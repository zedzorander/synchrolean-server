using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchroLean.Controllers.Resources;
using SynchroLean.Models;
using SynchroLean.Persistence;

namespace SynchroLean.Controllers
{
    [Route("api/[controller]")]
    public class TasksController : Controller
    {

        private readonly SynchroLeanDbContext context;

        public TasksController(SynchroLeanDbContext context)
        {
            this.context = context;    
        }

        // POST api/tasks
        [HttpPost]
        public async Task<IActionResult> AddUserTaskAsync([FromBody]UserTaskResource userTaskResource)
        {
            // How does this validate against the UserTask model?
            if(!ModelState.IsValid) {
                return BadRequest();
            }

            // Map resource to model
            var userTask = new UserTask {
                Id = userTaskResource.Id,
                Name = userTaskResource.Name,
                Description = userTaskResource.Description,
                IsRecurring = userTaskResource.IsRecurring,
                Weekdays = userTaskResource.Weekdays,
                // We'll need to think about timezones here
                CreationDate = DateTime.Now,
                IsCompleted = false,
                // Should we instead use a nullable datetime type?
                CompletionDate = DateTime.MinValue,
                IsRemoved = false
            };

            // Save userTask to database
            await context.AddAsync(userTask);
            await context.SaveChangesAsync();

            // Retrieve userTask from database
            userTask = await context.UserTasks
                .SingleOrDefaultAsync(ut => ut.Id == userTask.Id);

            // Map userTask to UserTaskResource
            var outResource = new UserTaskResource {
                Id = userTask.Id,
                Name = userTask.Name,
                Description = userTask.Description,
                IsRecurring = userTask.IsRecurring,
                Weekdays = userTask.Weekdays,
                CreationDate = userTask.CreationDate,
                IsCompleted = userTask.IsCompleted,
                CompletionDate = userTask.CompletionDate,
                IsRemoved = userTask.IsRemoved
            };

            return Ok(outResource);
        }

        // GET api/tasks
        [HttpGet]
        public async Task<IActionResult> GetTasksAsync()
        {
            // Fetch all tasks from the DB asyncronously
            var tasks = await context.UserTasks.ToListAsync<UserTask>();

            // List of corresponding tasks as resources
            var resourceTasks = new List<UserTaskResource>();

            // Map each task to a corresponding resource
            tasks.ForEach(task =>
            {
                // Create resource from model
                var resource = new UserTaskResource {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    IsRecurring = task.IsRecurring,
                    Weekdays = task.Weekdays,
                    CreationDate = task.CreationDate,
                    IsCompleted = task.IsCompleted,
                    CompletionDate = task.CompletionDate,
                    IsRemoved = task.IsRemoved
                };
                // Add to resources list
                resourceTasks.Add(resource);
            });
            return Ok(resourceTasks); // List of UserTaskResources 200OK
        }

        // PUT api/tasks
        [HttpPut("{id}")]
        public async Task<IActionResult> EditUserTaskAsync(int id, [FromBody]UserTaskResource userTaskResource)
        {
            // How does this validate against the UserTask model?
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var task = await context.UserTasks
                .SingleOrDefaultAsync(ut => ut.Id == id);

            // Nothing was retrieved, no id match
            if (task == null)
            {
                return NotFound();
            }

            // Map resource to model
            task.Name = userTaskResource.Name;
            task.Description = userTaskResource.Description;
            task.IsRecurring = userTaskResource.IsRecurring;
            task.Weekdays = userTaskResource.Weekdays;
            if (!task.IsCompleted && userTaskResource.IsCompleted) {
                // We'll need to think about timezones here
                task.CompletionDate = DateTime.Now;
            }
            task.IsCompleted = userTaskResource.IsCompleted;
            task.IsRemoved = userTaskResource.IsRemoved;

            // Save updated userTask to database
            await context.SaveChangesAsync();

            // Map userTask to UserTaskResource
            var outResource = new UserTaskResource
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                IsRecurring = task.IsRecurring,
                Weekdays = task.Weekdays,
                CreationDate = task.CreationDate,
                IsCompleted = task.IsCompleted,
                CompletionDate = task.CompletionDate,
                IsRemoved = task.IsRemoved
            };

            return Ok(outResource);
        }
    }
}
