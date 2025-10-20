using DevBoard.Domain.Entities;
using DevBoard.Domain.Enums;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBoard.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            await dbContext.Database.EnsureCreatedAsync();

            if (!await dbContext.Projects.IgnoreQueryFilters().AnyAsync())
            {
                var projects = GetMockProjects();

                await dbContext.Projects.AddRangeAsync(projects);
                await dbContext.SaveChangesAsync();
            }
        }

        private static List<Project> GetMockProjects()
        {
            return new List<Project>
            {
                new Project
                {
                    Name = "DevBoard Platform",
                    Description = "Internal development management platform.",
                    TenantId = Guid.NewGuid(),
                    Boards = new List<Board>
                    {
                        new Board
                        {
                            Name = "Development Board",
                            Description = "Tracks backend and API development tasks.",
                            Tasks = new List<TaskItem>
                            {
                                new TaskItem
                                {
                                    Name = "Implement Authentication",
                                    Description = "Add JWT authentication with Identity.",
                                    Status = TaskItemStatus.InProgress,
                                    DueDate = DateTime.UtcNow.AddDays(7)
                                },
                                new TaskItem
                                {
                                    Name = "Refactor Database Layer",
                                    Description = "Improve EF Core context and relationships.",
                                    Status = TaskItemStatus.Todo,
                                    DueDate = DateTime.UtcNow.AddDays(10)
                                }
                            }
                        },
                        new Board
                        {
                            Name = "UI/UX Design Board",
                            Description = "Manage design components and feedback.",
                            Tasks = new List<TaskItem>
                            {
                                new TaskItem
                                {
                                    Name = "Update Login Page",
                                    Description = "Redesign login page for accessibility.",
                                    Status = TaskItemStatus.Review,
                                    DueDate = DateTime.UtcNow.AddDays(5)
                                },
                                new TaskItem
                                {
                                    Name = "Dashboard Mockups",
                                    Description = "Prepare dashboard layout mockups.",
                                    Status = TaskItemStatus.InProgress,
                                    DueDate = DateTime.UtcNow.AddDays(3)
                                }
                            }
                        }
                    }
                },
                new Project
                {
                    Name = "Mobile App",
                    Description = "Mobile version of the DevBoard app.",
                    TenantId = Guid.NewGuid(),
                    Boards = new List<Board>
                    {
                        new Board
                        {
                            Name = "Feature Board",
                            Description = "Tracks new mobile app feature tasks.",
                            Tasks = new List<TaskItem>
                            {
                                new TaskItem
                                {
                                    Name = "Implement Notifications",
                                    Description = "Add push notifications for task updates.",
                                    Status = TaskItemStatus.Todo,
                                    DueDate = DateTime.UtcNow.AddDays(14)
                                },
                                new TaskItem
                                {
                                    Name = "Integrate API",
                                    Description = "Connect the app to the backend API.",
                                    Status = TaskItemStatus.InProgress,
                                    DueDate = DateTime.UtcNow.AddDays(8)
                                }
                            }
                        }
                    }
                }
            };
        }
    }

}
