using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct RankRenderInformation
{
    public Color Colour;
    public bool Top;
    public bool Middle;
    public bool Bottom;
    public bool Star;
}

public class RankRender : MonoBehaviour
{
    public Graphic Top;
    public Graphic Middle;
    public Graphic Bottom;
    public Graphic Star;

    public void RenderRank(Rank info)
    {
        RenderRank(info.Render);
    }

    public void RenderRank(RankRenderInformation info)
    {
        Top.color = info.Colour;
        Middle.color = info.Colour;
        Bottom.color = info.Colour;
        Star.color = info.Colour;

        Top.gameObject.SetActive(info.Top);
        Middle.gameObject.SetActive(info.Middle);
        Bottom.gameObject.SetActive(info.Bottom);
        Star.gameObject.SetActive(info.Star);
    }
}
