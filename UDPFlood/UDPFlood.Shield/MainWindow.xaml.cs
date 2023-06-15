using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NetFwTypeLib;

namespace UDPFlood.Shield;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        tbBlock.Click += (_, _) => BlockIp();
        btnUpdate.Click += (_, _) => Update();
        Update();
    }

    public void Update()
    {
        var policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
        var policyInstance = policyType != null ? Activator.CreateInstance(policyType) as INetFwPolicy2 : null;

        if (policyInstance == null)
            throw new NullReferenceException();

        var ruleList = new List<FirewallRule>();

        foreach (var ruleObject in policyInstance.Rules)
        {
            if (ruleObject is not INetFwRule rule
                || !rule.Enabled
                || rule.Direction != NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN
                || rule.Protocol != 17
                || rule.Action != NET_FW_ACTION_.NET_FW_ACTION_BLOCK)
                continue;

            ruleList.Add(new FirewallRule(rule.Name, rule.Description, rule.RemoteAddresses));
        }

        dgRules.ItemsSource = ruleList;
    }

    public void BlockIp()
    {
        var name = tbName.Text;
        var desc = tbDesc.Text;
        var ipText = tbIp.Text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(desc) || !IPAddress.TryParse(ipText, out var ip))
        {
            MessageBox.Show("Неправильный ввод данных!");
            return;
        }

        var policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
        var policyInstance = policyType != null ? Activator.CreateInstance(policyType) as INetFwPolicy2 : null;

        if (policyInstance == null)
            throw new NullReferenceException();

        var instanceType = Type.GetTypeFromProgID("HNetCfg.FWRule");
        var instance = instanceType != null ? Activator.CreateInstance(instanceType) : null;

        if (instance == null)
            throw new NullReferenceException();

        var firewallRule = (INetFwRule)instance;

        firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
        firewallRule.Enabled = true;
        firewallRule.InterfaceTypes = "All";
        firewallRule.RemoteAddresses = ipText;
        firewallRule.Name = name;
        firewallRule.Description = desc;
        firewallRule.Protocol = 17;

        policyInstance.Rules.Add(firewallRule);
        Update();
    }
}

public class FirewallRule
{
    public string Name { get; }
    public string Description { get; }
    public string Adresses { get; }

    public FirewallRule(string name, string description, string adresses)
    {
        Name = name;
        Description = description;
        Adresses = adresses;
    }
}
