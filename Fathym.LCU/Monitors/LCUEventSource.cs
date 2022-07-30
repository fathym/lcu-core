using Fathym.Design.Factory;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.LCU.Monitors
{
    public abstract class LCUEventSource : EventSource
    {
        #region Fields
        protected EventCounter counter;

        protected readonly int eventId;
        #endregion

        #region Properties

        #endregion

        #region Singleton
        #endregion

        #region Constructors
        protected LCUEventSource(string name, int eventId, Action<EventCounter> counterOptions = null)
        {
            counter = new EventCounter(name, this)
            {
                DisplayName = GetType().FullName,
                DisplayUnits = "ms"
            };

            if (counterOptions != null)
                counterOptions(counter);

            this.eventId = eventId;
        }
        #endregion

        #region API Methods
        public virtual void Request(params object[] args)
        {
            WriteEvent(eventId, args);
        }
        #endregion

        #region Helpers
        protected override void Dispose(bool disposing)
        {
            counter.Dispose();

            counter = null;

            base.Dispose(disposing);
        }
        #endregion
    }
}
