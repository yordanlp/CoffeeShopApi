using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CoffeeShopApi.ActionFilters {

    public enum RateLimitType {
        IP, USER
    }

    public class RateLimitAttribute : ActionFilterAttribute {
        private readonly int _limit;
        private readonly int _expirationMinutes;
        private readonly RateLimitType _type;

        public RateLimitAttribute(int limit, int expirationMinutes, RateLimitType type)
        {
            _limit = limit;
            _expirationMinutes = expirationMinutes;
            _type = type;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            
            var dbContext = (AppDbContext)context.HttpContext.RequestServices.GetService(typeof(AppDbContext));

            try
            {
                var key = GetKey(context.HttpContext);
                var strategy = dbContext.Database.CreateExecutionStrategy();
                strategy.Execute(() =>
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            dbContext.Database.ExecuteSqlRaw("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;");

                            var request = dbContext.RequestsPerKey.FirstOrDefault(r => r.Key == key);
                            if (request is not null)
                            {
                                if ((DateTime.Now - request.DateTime.Value).TotalMinutes < _expirationMinutes)
                                {
                                    if (request.Total > _limit)
                                    {
                                        context.Result = new StatusCodeResult((int)HttpStatusCode.TooManyRequests);
                                    }
                                    else
                                    {
                                        request.Total++;
                                        dbContext.Entry(request).State = EntityState.Modified;
                                    }

                                }
                                else
                                {
                                    request.Total = 1;
                                    request.DateTime = DateTime.Now;
                                }
                            }
                            else
                            {
                                request = new Models.RequestPerKey { Key = key, Total = 1, DateTime = DateTime.Now };
                                dbContext.RequestsPerKey.Add(request);
                            }

                            dbContext.SaveChanges();
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                        }
                    }
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            base.OnActionExecuting(context);
        }

        private string GetKey(HttpContext context)
        {
            switch( _type)
            {
                case RateLimitType.IP:
                    return context.Connection.RemoteIpAddress.ToString();
                case RateLimitType.USER:
                    return context.Request.Headers["Authorization"];
                default: throw new NotImplementedException();
            }
        }
    }
}
