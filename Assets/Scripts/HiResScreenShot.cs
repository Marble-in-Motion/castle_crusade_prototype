using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System.Threading;

public class HiResScreenShot : NetworkBehaviour
{
    private int resWidth = 224;
    private int resHeight = 224;

    public static string ScreenShotNameRealTimeData(int width, int height, int index, int teamId)
    {
        string directory = Path.GetFullPath(".");
        return string.Format("{0}/Screenshots/Team{1}/screen_{2}.png",
                             directory,
                             teamId,
                             index);
    }

    public static string ScreenShotNameTrainData(int width, int height, int index, int dangerIndex)
    {
        string directory = Path.GetFullPath(".");
        return string.Format("{0}/Screenshots/ScreenShots/{1}/screen_{2}_{3}.png",
                             directory,
                             dangerIndex,
                             index,
                             Random.Range(0, 1000000));
    }




    public void TakeScreenShotsTrain(GameObject[] players,int[] dangers)
    {
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
            string filename = ScreenShotNameTrainData(resWidth, resHeight, playerId, dangers[i]);

            Thread takeScreenShots = new Thread(() => ScreenShotThread(filename, bytes));
            takeScreenShots.Start();
        }
    }

    private void ScreenShotThread(string filename, byte[] bytes)
    {
        
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    public void TakeScreenShotsRealTime(GameObject[] players, int teamId)
    {
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
            string filename  = ScreenShotNameRealTimeData(resWidth, resHeight, playerId, teamId);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
        }
    }

}
