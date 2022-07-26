using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Hunt_MMR
{
    /* Hunt MMR Reference
     *     
    1 star: between 0 and 2000 MMR.
    2 stars: between 2000 and 2300 MMR.
    3 stars: between 2300 and 2600 MMR.
    4 stars: between 2600 and 2750 MMR.
    5 stars: between 2750 and 3000 MMR.
    6 stars: between 3000 and 5000 MMR.
    *
    */

    [PluginActionId("com.k-rol.huntmmr")]
    public class PluginAction : PluginBase
    {

        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.AttributesPath = String.Empty;
                instance.HuntPlayerName = String.Empty;
                return instance;
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "attributesPath")]
            public string AttributesPath { get; set; }

            [JsonProperty(PropertyName = "huntPlayerName")]
            public string HuntPlayerName { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public PluginAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                SaveSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
        }

        public override void KeyReleased(KeyPayload payload) 
        {
            
        }

        public async override void OnTick() 
        {
            string mmr = GetMMR();
            await Connection.SetTitleAsync(Environment.NewLine + Environment.NewLine + mmr + Environment.NewLine + CalculateStars(mmr));
        }

        private string CalculateStars(string mmr)
        {
            int mmr_int = int.Parse(mmr);
            string stars = string.Empty;

            if (mmr_int <= 2000)
                stars = "*";

            else if (mmr_int > 2000 && mmr_int <= 2300)
                stars = "**";

            else if (mmr_int > 2300 && mmr_int <= 2600)
                stars = "***";

            else if (mmr_int > 2600 && mmr_int <= 2750)
                stars = "****";

            else if (mmr_int > 2750 && mmr_int <= 3000)
                stars = "*****";

            else if (mmr_int > 3000)
                stars = "******";
            return stars;
        }
        private string GetMMR()
        {
            //Extract MMR from attributes.xml
            string teamPrefix = String.Empty;
            string teamSuffix = "_blood_line_name";
            string playerNodeName = string.Empty;
            string MMR = String.Empty;

            FixURL();

            //Load File Content
            string attributeContent = File.ReadAllText(settings.AttributesPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(attributeContent);

            XmlNodeList nodelist = xmlDoc.ChildNodes;
            //teamName = every child nodes which _.name is like *_ownteam AND _.value == true
            foreach (XmlNode node in nodelist[0].ChildNodes)
            {
                XmlElement element = (XmlElement)node;
                if (element.GetAttribute("name").Contains("_ownteam") && element.GetAttribute("value") == "true")
                {
                    teamPrefix = element.GetAttribute("name");
                    break;
                }
            }

            //(MissionBagPlayer_0)	
            teamPrefix = teamPrefix.Replace("Team", "Player").Replace("_ownteam", "");

            //PlayerName => For every node with name = MissionBagPlayer_0_?_blood_line_name
            //              If(_.value == "toolonglyf") return _.name(MissionBagPlayer_0_1_blood_line_name)
            foreach (XmlNode node in nodelist[0].ChildNodes)
            {
                XmlElement element = (XmlElement)node;
                if (element.GetAttribute("name").Contains(teamPrefix) && element.GetAttribute("name").Contains(teamSuffix) && element.GetAttribute("value") == settings.HuntPlayerName)
                {
                    playerNodeName = element.GetAttribute("name");
                }
            }

            //MMR = allnodes.node[name = MissionBagPlayer_0_1_mmr].value
            foreach (XmlNode node in nodelist[0].ChildNodes)
            {
                string mmrNodeNade = playerNodeName.Replace(teamSuffix, "_mmr");
                XmlElement element = (XmlElement)node;
                if (element.GetAttribute("name") == mmrNodeNade)
                {
                    MMR = element.GetAttribute("value");
                }

            }
            return MMR;
        }

        private void FixURL()
        {
            string newPath = WebUtility.UrlDecode(settings.AttributesPath);
            newPath = newPath.Replace(@"C:\fakepath\", "");
            settings.AttributesPath = newPath;

            //Logger.Instance.LogMessage(TracingLevel.INFO, Message: settings.HuntPlayerName + " : " + settings.AttributesPath);
            SaveSettings();
        }

        /*        public override void ReceivedSettings(ReceivedSettingsPayload payload)
                {
                    Tools.AutoPopulateSettings(settings, payload.Settings);
                    SaveSettings();
                }*/

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            // Return fixed filename back to the Property Inspector
            await Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion

    }
}