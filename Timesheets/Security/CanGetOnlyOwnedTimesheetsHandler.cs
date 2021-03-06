﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Timesheets.Data;
using Timesheets.Models;

namespace Timesheets.Security
{
    public class CanGetOnlyOwnedTimesheetsHandler : AuthorizationHandler<OperationAuthorizationRequirement, TimesheetEntry>
    {

        ApplicationDbContext _context;

        public CanGetOnlyOwnedTimesheetsHandler(ApplicationDbContext context) {
            this._context = context;
            
            
        }

        protected override  Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, TimesheetEntry resource)
        {
           
        
            if (context.User == null || resource == null)
            {
                // Return Task.FromResult(0) if targeting a version of
                // .NET Framework older than 4.6:
                return  Task.CompletedTask;
            }

            // If we're not asking for CRUD permission, return.

            if (requirement.Name != Constants.CreateOperationName &&
                requirement.Name != Constants.ReadOperationName &&
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            {
                return Task.CompletedTask;
            }

            if (context.User.IsInRole("Admin")) {
                context.Succeed(requirement);
            }
            
            if (context.User.IsInRole("Manager")) {
                var actualResource = _context.TimesheetEntries.Include(d => d.RelatedUser).Include(d=>d.RelatedUser.Department).FirstOrDefault(i => i.Id == resource.Id);
                if (actualResource.RelatedUser.Department.DepartmentHeadId != null)
                {
                    if (actualResource.RelatedUser.Department.DepartmentHeadId == context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value)
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            if (context.User.IsInRole("Employee"))
            {
                if (resource.RelatedUser.Id == context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;

        }

    }
    }

