﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchroLean.Controllers.Resources;
using SynchroLean.Core.Models;
using SynchroLean.Persistence;
using SynchroLean.Core;
using Microsoft.AspNetCore.Authorization;

namespace SynchroLean.Controllers
{
    /// <summary>
    /// This class handles HTTP requests for teams
    /// </summary>
    [Route("api/[controller]")]
    public class TeamController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper _mapper;
        public TeamController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // POST api/team
        /// <summary>
        /// Adds a new team to the Db asynchronously
        /// </summary>
        /// <param name="teamResource"></param>
        /// <returns>A resource of the new team</returns>
        [HttpPost, Authorize]
        public async Task<IActionResult> AddTeamAsync([FromBody]TeamResource teamResource)
        {
            // Validate against the team model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that the user creating the team is the assigned owner
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(teamResource.OwnerEmail)) 
            {
                return Forbid();
            }

            // Map the team resource to a model
            var teamModel = _mapper.Map<Team>(teamResource);

            // Add the team to context and save changes
            await unitOfWork.UserTeamRepository.AddAsync(teamModel);
            await unitOfWork.CompleteAsync();

            // Fetch the newly created team from the DB
            var team = await unitOfWork.UserTeamRepository
                .GetUserTeamAsync(teamModel.Id);

            return Ok(_mapper.Map<TeamResource>(team)); // Return newly created mapped team resource to client
        }

        /// <summary>
        /// Acts as a get all teams method. The reasoning for this is... if a user
        /// is supposed to be able to view aggregate metrics for other teams then
        /// they would have to be able to get all the teams. This can be modified
        /// for various scenarios... E.G. maybe a team owner can look at other 
        /// teams aggregate metrics...
        /// </summary>
        /// <returns>A list of all teams</returns>
        // GET api/team
        [HttpGet, Authorize]
        public async Task<IActionResult> GetTeamsAsync()
        {
            // Fetch all teams from the database
            var teams = await unitOfWork.UserTeamRepository
                .GetAllTeamsAsync();

            // List of resource versions of teams
            var resourceTeams = new List<TeamResource>();

            // Map each team to a resource
            foreach (var team in teams)
            {
                resourceTeams.Add(_mapper.Map<TeamResource>(team));
            }

            return Ok(resourceTeams); // Return the collection of team resources
        }
        /// <summary>
        /// Get the given team.
        /// </summary>
        /// <param name="teamId">The id for the team to fetch.</param>
        /// <returns>A team resource</returns>
        // GET api/team/tid
        [HttpGet("{teamId}"), Authorize]
        public async Task<IActionResult> GetUserTeamAsync(int teamId)
        {
            // Get the team for the currently logged in user
            var team = await unitOfWork.UserTeamRepository
                .GetUserTeamAsync(teamId);

            // Check to see if a team corresponding to the given team id was found
            if (team == null)
            {
                return NotFound("Couldn't find a team matching that id."); // Team wasn't found
            }

            return Ok(_mapper.Map<TeamResource>(team)); // Return mapped team to client
        }

        /// <summary>
        /// Get a list of all members for a team.
        /// </summary>
        /// <param name="teamId">The team for which to get members.</param>
        /// <returns></returns>
        [HttpGet("members/{teamId}"), Authorize]
        public async Task<IActionResult> GetTeamMembers(int teamId)
        {
            // Get the team for the currently logged in user
            var team = await unitOfWork.UserTeamRepository
                .GetUserTeamAsync(teamId);

            // Check to see if a team corresponding to the given team id was found
            if (team == null)
            {
                return NotFound("Couldn't find a team matching that id."); // Team wasn't found
            }

            var teamMembers = await unitOfWork.TeamMemberRepository.GetAllUsersForTeam(teamId);
            return Ok(teamMembers.Select(member => _mapper.Map<UserAccountResource>(member)));
        }

        // PUT api/team/ownerId/teamId
        /// <summary>
        /// Updates an existing team in the Db
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="teamResource"></param>
        /// <returns>A resource of updated team</returns>
        [HttpPut("{teamId}"), Authorize]
        public async Task<IActionResult> UpdateUserTeamAsync(int teamId, [FromBody]TeamResource teamResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that the user editing the team is the owner
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(teamResource.OwnerEmail))
            {
                return Forbid();
            }

            // Fetch an account from the DB asynchronously
            var account = await unitOfWork.UserAccountRepository
                .GetUserAccountAsync(tokenOwnerEmail);

            // Return not found exception if account doesn't exist
            if (account == null)
            {
                return NotFound("Couldn't find an account matching that ownerId.");
            }

            // Get the team for the currently logged in user
            var team = await unitOfWork.UserTeamRepository
                .GetUserTeamAsync(teamId);

            // Nothing was retrieved, no id match
            if (team == null)
            {
                return NotFound("Couldn't find a team matching that teamId.");
            }

            // Map resource to model
            team.TeamName = teamResource.TeamName;
            team.TeamDescription = teamResource.TeamDescription;

            await unitOfWork.TeamMemberRepository.ChangeTeamOwnership(teamId, teamResource.OwnerEmail);

            //this stops default edit team from giving teams to user 0
            if (team.OwnerEmail == null)
            {
                team.OwnerEmail = tokenOwnerEmail;
            }

            // Save updated team to database
            await unitOfWork.CompleteAsync();

            // Return mapped team resource
            return Ok(_mapper.Map<TeamResource>(team));
        }
        /// <summary>
        /// Create a new invite for a user.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        // PUT api/team/invite/
        [HttpPut("invite/{emailAddress}/{teamId}"), Authorize]
        public async Task<IActionResult> InviteUserToTeamAsync(string emailAddress, int teamId)
        {
            var team = await unitOfWork.UserTeamRepository.GetUserTeamAsync(teamId);
            if (team == null || team.IsDeleted) return NotFound("No such team");
            //TODO: Check if creator is in the team creator is being invited into
            //blocked by the fact that teams aren't implemented yet
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            var creatorIsTeamOwner = team.OwnerEmail == tokenOwnerEmail;
            var inviter = await unitOfWork.UserAccountRepository.GetUserAccountAsync(tokenOwnerEmail);
            if (inviter == null || inviter.IsDeleted) return NotFound("User doesn't exist");
            var invitee = await unitOfWork.UserAccountRepository.GetUserAccountAsync(emailAddress);
            if (invitee == null || invitee.IsDeleted) return NotFound("User doesn't exist");
            await unitOfWork.AddUserRequestRepository.AddAsync(
                new AddUserRequest
                {
                    Invitee = invitee,
                    Inviter = inviter,
                    IsAuthorized = creatorIsTeamOwner,
                    DestinationTeam = team
                });
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Accept a user's authorized invite.
        /// </summary>
        /// <param name="addUserRequestId">The invite being accepted.</param>
        /// <returns></returns>
        [HttpPut("invite/accept/{addUserRequestId}"), Authorize]
        public async Task<IActionResult> AcceptTeamInvite(int addUserRequestId)
        {
            var invite = await unitOfWork.AddUserRequestRepository.GetAddUserRequestAsync(addUserRequestId);
            if (invite == null || !invite.IsAuthorized) return NotFound("No such invite");

            // Validate that the accepting user is the invitee
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(invite.Invitee.Email)) 
            {
                return Forbid();
            }

            await unitOfWork.TeamMemberRepository.AddUserToTeam(invite.DestinationTeam.Id, invite.Invitee.Email);
            await unitOfWork.AddUserRequestRepository.DeleteAddUserRequestAsync(addUserRequestId);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Reject a specific invitation for your account
        /// </summary>
        /// <param name="addUserRequestId"></param>
        /// <returns></returns>
        [HttpPut("invite/reject/{addUserRequestId}"), Authorize]
        public async Task<IActionResult> RejectTeamInvite(int addUserRequestId)
        {
            var invite = await unitOfWork.AddUserRequestRepository.GetAddUserRequestAsync(addUserRequestId);
            if (invite == null || !invite.IsAuthorized) return NotFound("No such invite");
            
            // Validate that the rejecting user is the invitee
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(invite.Invitee.Email)) 
            {
                return Forbid();
            }

            await unitOfWork.AddUserRequestRepository.DeleteAddUserRequestAsync(addUserRequestId);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Rescind an invitation you gave someone.
        /// </summary>
        /// <param name="addUserRequestId"></param>
        /// <returns></returns>
        [HttpPut("invite/rescind/{addUserRequestId}"), Authorize]
        public async Task<IActionResult> RescindTeamInvite(int addUserRequestId)
        {
            var invite = await unitOfWork.AddUserRequestRepository.GetAddUserRequestAsync(addUserRequestId);
            if (invite == null) return NotFound("No such invite");

            // Validate that the rescinding user is the inviter
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(invite.Inviter.Email)) 
            {
                return Forbid();
            }

            await unitOfWork.AddUserRequestRepository.DeleteAddUserRequestAsync(addUserRequestId);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Authorize an invitation by a team member
        /// </summary>
        /// <param name="addUserRequestId"></param>
        /// <returns></returns>
        [HttpPut("invite/authorize/{addUserRequestId}"), Authorize]
        public async Task<IActionResult> AuthorizeTeamInvite(int addUserRequestId)
        {
            var invite = await unitOfWork.AddUserRequestRepository.GetAddUserRequestAsync(addUserRequestId);
            if (invite == null) return NotFound("No such invite");
            // Note: ensure up cascade deletion for team invites, we should be able to assume the team exists here
            var team = await unitOfWork.UserTeamRepository.GetUserTeamAsync(invite.DestinationTeam.Id);
            if (team == null) return NotFound("No such team");

            // Validate that the authorizing user is the team owner
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(team.OwnerEmail)) 
            {
                return Forbid();
            }

            invite.IsAuthorized = true;
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Veto an invitation by a team member
        /// </summary>
        /// <param name="addUserRequestId"></param>
        /// <returns></returns>
        [HttpPut("invite/veto/{addUserRequestId}/{ownerId}"), Authorize]
        public async Task<IActionResult> VetoTeamInvite(int addUserRequestId)
        {
            var invite = await unitOfWork.AddUserRequestRepository.GetAddUserRequestAsync(addUserRequestId);
            if (invite == null) return NotFound("No such invite");
            // Note: ensure up cascade deletion for team invites, we should be able to assume the team exists here
            var team = await unitOfWork.UserTeamRepository.GetUserTeamAsync(invite.DestinationTeam.Id);
            if (team == null) return NotFound("No such team");
            
            // Validate that the vetoing user is the team owner
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(team.OwnerEmail)) 
            {
                return Forbid();
            }

            await unitOfWork.AddUserRequestRepository.DeleteAddUserRequestAsync(addUserRequestId);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Get all the pending invites that can be accepted by a given user.
        /// </summary>
        /// <returns>All invites that the user may accept.</returns>
        [HttpGet("invite/incoming/accept"), Authorize]
        public async Task<IActionResult> GetInvitesToAccept()
        {
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            var userExists = await unitOfWork.UserAccountRepository.UserAccountExists(tokenOwnerEmail);
            if (!userExists) return NotFound("No such user");

            var invites = await unitOfWork.AddUserRequestRepository.GetAddUserRequestsPendingAcceptanceAsync(tokenOwnerEmail);
            return Ok(invites.Select(inv => new AddUserRequestResource(inv)));
        }

        /// <summary>
        /// Get all the pending invites a user can authorize.
        /// </summary>
        /// <returns>All invites that the user may authorize.</returns>
        [HttpGet("invite/incoming/authorize"), Authorize]
        public async Task<IActionResult> GetInvitesToAuthorize()
        {
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            var userExists = await unitOfWork.UserAccountRepository.UserAccountExists(tokenOwnerEmail);
            if (!userExists) return NotFound("No such user");

            var invites = await unitOfWork.AddUserRequestRepository.GetAddUserRequestsPendingApprovalAsync(tokenOwnerEmail);
            return Ok(invites.Select(inv => new AddUserRequestResource(inv)));
        }

        /// <summary>
        /// Get all the pending invites a user has created and can rescind.
        /// </summary>
        /// <returns>All invites that the user has created and may rescind.</returns>
        [HttpGet("invite/outgoing"), Authorize]
        public async Task<IActionResult> GetCreatedInvites()
        {
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            var userExists = await unitOfWork.UserAccountRepository.UserAccountExists(tokenOwnerEmail);
            if (!userExists) return NotFound("No such user");

            var invites = await unitOfWork.AddUserRequestRepository.GetMySentAddUserRequestsAsync(tokenOwnerEmail);
            return Ok(invites.Select(inv => new AddUserRequestResource(inv)));
        }

        /// <summary>
        /// Permit a team to see detailed stats on one of a user's teams.
        /// </summary>
        /// <param name="subjectId">The team for which permissions will be granted.</param>
        /// <param name="objectId">The user's team.</param>
        /// <returns></returns>
        [HttpPut("permissions/grant/{objectId}/{subjectId}"), Authorize]
        public async Task<IActionResult> PermitTeamToSee(int subjectId, int objectId)
        {
            // -- We don't need to actually check who the user is, but if you want to, uncomment this
            //var ownerExists = await unitOfWork.userAccountRepository.UserAccountExists(ownerId);
            //if (!ownerExists) return NotFound("No such user");
            var subjectTeam = await unitOfWork.UserTeamRepository.GetUserTeamAsync(subjectId);
            if (subjectTeam == null || subjectTeam.IsDeleted) return NotFound("No such team (subjectId)");
            var objectTeam = await unitOfWork.UserTeamRepository.GetUserTeamAsync(objectId);
            if (objectTeam == null || objectTeam.IsDeleted) return NotFound("No such team (objectId)");

            // Validate that the user granting permissions owns the object team
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(objectTeam.OwnerEmail)) 
            {
                return Forbid();
            }

            await unitOfWork.TeamPermissionRepository.Permit(subjectId, objectId);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Forbid a team from seeing detailed stats on one of a user's teams.
        /// </summary>
        /// <param name="subjectId">The team for which permissions will be revoking.</param>
        /// <param name="objectId">The user's team.</param>
        /// <returns></returns>
        [HttpPut("permissions/revoke/{objectId}/{subjectId}"), Authorize]
        public async Task<IActionResult> ForbidTeamToSee(int subjectId, int objectId)
        {
            // -- We don't need to actually check who the user is, but if you want to, uncomment this
            //var ownerExists = await unitOfWork.userAccountRepository.UserAccountExists(ownerId);
            //if (!ownerExists) return NotFound("No such user");
            var subjectExists = await unitOfWork.UserTeamRepository.TeamExists(subjectId);
            if (!subjectExists) return NotFound();
            var objectTeam = await unitOfWork.UserTeamRepository.GetUserTeamAsync(objectId);
            if (objectTeam == null) return NotFound("No such team (objectId)");

            // Validate that the user revoking permissions owns the object team
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(objectTeam.OwnerEmail)) 
            {
                return Forbid();
            }

            await unitOfWork.TeamPermissionRepository.Forbid(subjectId, objectId);
            await unitOfWork.CompleteAsync();
            return Ok();
        }        

        /// <summary>
        /// Removes a user from a team, except a team owner (currently)
        /// </summary>
        /// <param name="targetEmail">The user to be removed from the team.</param>
        /// <param name="teamId"> The team the user is too be removed from.</param>
        /// <returns></returns>
        [HttpPut("remove/{targetEmail}/{teamId}"), Authorize]
        public async Task<IActionResult> RemoveMemberAsync(string targetEmail, int teamId){
        
            var targetTeam = await unitOfWork.UserTeamRepository
                .GetUserTeamAsync(teamId);
            
            if (targetTeam == null){
                return NotFound("Not a valid team.");
            }

            // Validate that the user removing another user is either the team owner
            // or they are removing themselves (leaving the team).
            var tokenOwnerEmail = User.FindFirst("Email").Value;
            if (!tokenOwnerEmail.Equals(targetTeam.OwnerEmail) && !tokenOwnerEmail.Equals(targetEmail)) 
            {
                return Forbid();
            }

            if(targetTeam.OwnerEmail == targetEmail){
                return BadRequest("Can't remove the team owner");
            }

            await unitOfWork.TeamMemberRepository.RemoveUserFromTeam(teamId,targetEmail);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        /// <summary>
        ///  Returns a list of tasks for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        [HttpGet("rollup/{teamId}"), Authorize]
        public async Task<IActionResult> TeamRollUpAsync(int teamId){
                        
            var team = await unitOfWork.UserTeamRepository
                .GetUserTeamAsync(teamId);

            if (team == null || team.IsDeleted){
                return NotFound("Not a valid team.");
            }

            var tokenOwnerEmail = User.FindFirst("Email").Value;
            var userCanSeeTeam = await unitOfWork.TeamPermissionRepository.UserIsPermittedToSeeTeam(tokenOwnerEmail, teamId);
            if (!userCanSeeTeam)
            {
                return Forbid();
            }
            
            var teamUserTasks = await unitOfWork.UserTaskRepository.GetTeamTasksAsync(teamId);
            return Ok(teamUserTasks.Select(task => _mapper.Map<UserTaskResource>(task)));      
        }

        /// <summary>
        /// Allow a team owner to delete a team. By popular demand.
        /// </summary>
        /// <param name="teamId">The team to be deleted.</param>
        /// <returns></returns>
        [HttpPost("delete/{teamId}"), Authorize]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            var team = await unitOfWork.UserTeamRepository.GetUserTeamAsync(teamId);

            if (team == null || team.IsDeleted)
            {
                return NotFound("Not a valid team.");
            }

            var tokenOwnerEmail = User.FindFirst("Email").Value;

            if (team.OwnerEmail != tokenOwnerEmail) return Forbid();
            else
            {
                await unitOfWork.UserTeamRepository.DeleteTeamAsync(teamId);
                return Ok();
            }
        }
    }
}


