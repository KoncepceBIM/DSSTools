using LOIN.Context;
using LOIN.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Controllers
{
    public abstract class ContextController<T>: ControllerBase where T: IContextEntity
    {
        protected ContextController(ILogger logger)
        {
            Logger = logger;
        }

        protected ILogger Logger { get; }

        protected ILoinModel Model => HttpContext.Items[Constants.RepositoryContextKey] as ILoinModel;

    }
}
