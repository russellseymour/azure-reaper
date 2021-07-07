
using System.Collections.Generic;

namespace Azure.Reaper.Lib.Models
{
    public class PowerStatistics
    {

        private List<string> vms_started;
        private List<string> vms_stopped;
        private List<string> aks_started;
        private List<string> aks_stopped;

        public PowerStatistics() {
            vms_started = new List<string>();
            vms_stopped = new List<string>();
            aks_started = new List<string>();
            aks_stopped = new List<string>();
        }

        // create methods to add the items to the lists
        public void StartResource(string type, string name)
        {
            switch (type)
            {
                case "vm":
                    vms_started.Add(name);
                    break;

                case "aks":
                    aks_started.Add(name);
                    break;
            }
        }

        public void StopResource(string type, string name)
        {
            switch (type)
            {
                case "vm":
                    vms_stopped.Add(name);
                    break;

                case "aks":
                    aks_stopped.Add(name);
                    break;
            }
        }

        public List<string> GetStarted(string type)
        {
            List<string> list = new List<string>();

            switch (type)
            {
                case "vm":
                    list = vms_started;
                    break;

                case "aks":
                    list = aks_started;
                    break;
            }

            return list;
        } 

        public List<string> GetStopped(string type)
        {
            List<string> list = new List<string>();

            switch (type)
            {
                case "vm":
                    list = vms_stopped;
                    break;

                case "aks":
                    list = aks_stopped;
                    break;
            }

            return list;
        }         
    }
}