using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsaacLauncherGUI
{
    public class GameStartupMonitor
    {
        public static async Task<GameStartupResult> WaitForGameStartupAsync(bool isRepentogonEnabled)
        {
            GameStartupResult gameStartupResult = new GameStartupResult();
            // 0. Ha a Repentogon be van kapcsolva, akkor megvárjuk, hogy elinduljon a REPENTOGONLauncher.exe és megjelenjen az ablak
            if (isRepentogonEnabled)
            {
                Process Repentogon = await GameProcessMonitor.WaitForProcessAsync("REPENTOGONLauncher", 10000);
                if (Repentogon == null) 
                {
                    gameStartupResult.ErrorMessage = "Repentogon process did not start in time!";
                    return gameStartupResult;
                }

                bool repentogonWindowAppeared = await GameProcessMonitor.WaitForResponsiveAsync(Repentogon, 10000);
                if (!repentogonWindowAppeared) 
                {
                    gameStartupResult.ErrorMessage = "Repentogon did not become responsive in time!";
                    return gameStartupResult;
                }
            }

            // 1. Megvárjuk, amíg elindul a processz
            Process Isaac = await GameProcessMonitor.WaitForProcessAsync("isaac-ng", 60000);
            if (Isaac == null) 
            {
                gameStartupResult.ErrorMessage = "Isaac process did not start in time!";
                return gameStartupResult;
            }

            // 2. Várjuk meg az ablak handle-t
            bool windowAppeared = await GameProcessMonitor.WaitForWindowAsync(Isaac, 120000);
            if (!windowAppeared)
            {
                gameStartupResult.ErrorMessage = "Isaac window did not appear in time!";
                return gameStartupResult;
            }

            // 3. Várjuk meg, hogy reagáljon is (itt tölt be a Repentogon és a modok)
            bool responding = await GameProcessMonitor.WaitForResponsiveAsync(Isaac, 120000);
            if (!responding) 
            {
                gameStartupResult.ErrorMessage = "Isaac did not become responsive in time!";
                return gameStartupResult;
            }

            // 4. Ha minden rendben, akkor visszaadjuk a processzt
            gameStartupResult.Process = Isaac;
            return gameStartupResult;
        }
    }
}
