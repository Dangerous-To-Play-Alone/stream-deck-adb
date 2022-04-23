using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using BarRaider.SdTools.Wrappers;
using BarRaider.SdTools.Events;

namespace stream_deck_adb
{


    class AdbDevice
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; private set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        public AdbDevice(string deviceId, string deviceName)
        {
            Id = deviceId;
            Name = deviceName;
        }
    }

    [PluginActionId("stream.deck.adb.deeplink")]
    public class DeeplinkAction : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.InputUrl = "https://www.google.ca";
                instance.Devices = new List<AdbDevice>();
                return instance;
            }


            [JsonProperty(PropertyName = "deviceId")]
            public string DeviceId { get; set; }

            [JsonProperty(PropertyName = "devices")]
            public List<AdbDevice> Devices { get; set; }

            [JsonProperty(PropertyName = "inputUrl")]
            public string InputUrl { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public DeeplinkAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;
        }

        public override void Dispose()
        {
            Connection.OnPropertyInspectorDidAppear -= Connection_OnPropertyInspectorDidAppear;
        }

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
                string command = String.Format("adb shell am start -a android.intent.action.VIEW -d '{0}'", this.settings.InputUrl); 
                using (Process proc = new Process())
                {
                    ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe");
                    procStartInfo.RedirectStandardInput = true;
                    procStartInfo.RedirectStandardOutput = true;
                    procStartInfo.RedirectStandardError = true;
                    procStartInfo.UseShellExecute = false;
                    procStartInfo.CreateNoWindow = true;
                    proc.StartInfo = procStartInfo;
                    proc.Start();
                    proc.StandardInput.WriteLine(command);
                    proc.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, e.StackTrace);
            }
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<PropertyInspectorDidAppear> e)
        {
            String command = "adb devices";
            List<AdbDevice> devices = new List<AdbDevice>();
            using (Process proc = new Process())
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe");
                procStartInfo.RedirectStandardInput = true;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                //procStartInfo.CreateNoWindow = true;
                proc.StartInfo = procStartInfo;
                proc.Start();
                proc.StandardInput.WriteLine(command);

                //while (!proc.StandardOutput.EndOfStream)
                //{
                    devices.Add(new AdbDevice(Guid.NewGuid().ToString(), proc.StandardOutput.ReadLine()));
                //}
                proc.Close();
            }

            settings.Devices = devices;

            SaveSettings();
        }

        #endregion
    }
}