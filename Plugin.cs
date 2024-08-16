using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Loader;
using Exiled.Permissions;
using MEC;

namespace DAONRotation
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;

        public List<string> RotationModes = new List<string>() { "정전", "데스매치" };
        public string CurrentMode;
        public int CurrentModeIndex = 0;
        public bool RoundEnded = false;

        public override void OnEnabled()
        {
            Instance = this;

            Timing.RunCoroutine(ShowRoundServer());
            Timing.RunCoroutine(Rotation());

            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;

            base.OnDisabled();
            Instance = null;
        }

        public IEnumerator<float> ShowRoundServer()
        {
            yield return Timing.WaitForSeconds(10f);

            while (true)
            {
                if (Round.IsLobby)
                {
                    foreach (var player in Player.List)
                    {
                        string hintText = $"<align=right><b><size=30><color=#A4A4A4>DAON</color> <color=#00FFFF>로테이션</color> 서버</size></b>\n<size=20>{string.Join(", ", RotationModes).Replace($"{CurrentMode}", $"<color=yellow>{CurrentMode}</color>")}</size>\n\n\n\n\n\n\n\n\n\n\n\n\n</align>";
                        player.ShowHint(hintText, 1.2f);
                    }
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        public IEnumerator<float> Rotation()
        {
            while (true)
            {
                CurrentMode = RotationModes[CurrentModeIndex];

                ServerConsole.AddLog($"[DAONRotation] 이번 라운드는 ({CurrentMode} 서버)가 활성화되었습니다.", ConsoleColor.Yellow);

                if (CurrentMode == "데스매치")
                {
                    Loader.GetPlugin("DAONDeathMatch").OnEnabled();

                    while (!RoundEnded)
                        yield return Timing.WaitForSeconds(1f);

                    Loader.GetPlugin("DAONDeathMatch").OnDisabled();
                }
                else if (CurrentMode == "정전")
                {
                    Loader.GetPlugin("DAONPowerOutage").OnEnabled();

                    while (!RoundEnded)
                        yield return Timing.WaitForSeconds(1f);

                    Loader.GetPlugin("DAONPowerOutage").OnDisabled();
                }
                else
                {
                    ServerConsole.AddLog("[DAONRotation] 알 수 없는 서버입니다.", ConsoleColor.Red);
                }

                CurrentModeIndex++;

                if (CurrentModeIndex > RotationModes.Count() - 1)
                    CurrentModeIndex = 0;

                yield return Timing.WaitForSeconds(8f);

                RoundEnded = false;
            }
        }

        public void OnRoundEnded(Exiled.Events.EventArgs.Server.RoundEndedEventArgs ev)
        {
            RoundEnded = true;
        }
    }
}
