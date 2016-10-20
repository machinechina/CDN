using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CDN.Infrastructure
{
    public class UpdateCheckingWorker : Worker
    {
        public UpdateCheckingWorker(int loopInterval) : base(loopInterval)
        {
        }

        protected override void DoWork()
        {
            ApplicationHelper.CheckUpdate(() =>
            {
                Cancel();
            });
        }
    }
}
