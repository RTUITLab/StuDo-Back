﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace studo.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> logger;

        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            this.logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var modelState = context.ModelState;

            if (!modelState.IsValid)
            {
                context.HttpContext.Response.StatusCode = 400;
                context.Result = new JsonResult("Model is invalid");

                logger.LogDebug("Below should be a model in JSON format -->");
                try
                {
                    var body = BodyToString(context.HttpContext.Request);
                    logger.LogDebug(body);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex.Message);
                    logger.LogDebug(ex.StackTrace);
                }
                logger.LogDebug("Above should be a model in JSON format <--");
            }
        }

        private string BodyToString(HttpRequest request)
        {
            var returnValue = string.Empty;
            request.EnableRewind();
            //ensure we read from the begining of the stream - in case a reader failed to read to end before us.
            request.Body.Position = 0;
            //use the leaveOpen parameter as true so further reading and processing of the request body can be done down the pipeline
            using (var stream = new StreamReader(request.Body, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                returnValue = stream.ReadToEnd();
            }
            //reset position to ensure other readers have a clear view of the stream 
            request.Body.Position = 0;
            return returnValue;
        }
    }
}
