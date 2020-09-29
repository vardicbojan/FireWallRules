using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirewallRules
{
    class FirewallRule
    {
        private const string ruleName = "IISAndroidTestBlockConnection";

        internal void IISAndroidTestChangeRuleStatus(bool enabled)
        {
            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

            foreach (INetFwRule rule in fwPolicy2.Rules)
            {
                if (rule.Name == ruleName)
                    rule.Enabled = enabled;
            }
        }

        internal bool IsConnectionRuleDisabled()
        {
            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

            foreach (INetFwRule rule in fwPolicy2.Rules)
            {
                if (rule.Name == ruleName)
                    return rule.Enabled;
            }

            return false;
        }

        internal bool DoesRuleExists()
        {
            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

            foreach (INetFwRule rule in fwPolicy2.Rules)
            {
                if (rule.Name == ruleName)
                    return true;
            }

            return false;
        }
    }
}
