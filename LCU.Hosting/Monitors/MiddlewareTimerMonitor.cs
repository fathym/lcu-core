using LCU.Monitors;
using System.Collections.Generic;

namespace LCU.Hosting.Monitors
{
    public abstract class MiddlewareTimerMonitor : LCUEventSource
    {
        #region Constructors
        protected MiddlewareTimerMonitor(string name, int eventId)
            : base(name, eventId)
        { }
        #endregion

        #region API Methods
        public virtual void Request<TMiddleware>(float elapsedMilliseonds, params object[] args)
        {
            //var argsLst = new List<object>(args);

            //argsLst.Add(typeof(TMiddleware).FullName);

            //argsLst.Add(elapsedMilliseonds);

            //Request(elapsedMilliseonds, argsLst.ToArray());
        }
        #endregion
    }
}
