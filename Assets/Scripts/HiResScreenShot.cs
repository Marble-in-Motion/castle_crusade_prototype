using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

public class HiResScreenShot : NetworkBehaviour
{
    public int resWidth = 1280;
    public int resHeight = 720;

    private bool takeHiResShot = false;

    public static string ScreenShotName(int width, int height, int index, int teamId)
    {
        string directory = Path.GetFullPath(".");
        return string.Format("{0}/Screenshots/Team{1}/screen_{2}.png",
                             directory,
                             teamId,
                             index);
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    [Command]
    public void CmdTakeScreenShots(GameObject[] players, int teamId)
    {
        Debug.Log("Players:" + players.Length);
        for (int i = 0; i < players.Length; i++)
        {
            int playerId = players[i].GetComponent<Player>().GetId();
            Camera camera = players[i].GetComponentInChildren<Camera>();
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight, playerId, teamId);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
        }
    }

}
