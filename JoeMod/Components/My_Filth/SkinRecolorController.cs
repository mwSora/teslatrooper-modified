﻿using UnityEngine;

[System.Serializable]
public class Recolor {

    public string recolorName;

    public Color[] colors = new Color[] { Color.red };
}

[System.Serializable]
public class RecolorGroup {

    [SerializeField]
    private Renderer[] renderers;

    private MaterialPropertyBlock[] _matProperties;

    public void fillPropertieBlocks() {

        _matProperties = new MaterialPropertyBlock[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) {
            _matProperties[i] = new MaterialPropertyBlock();
            renderers[i].GetPropertyBlock(_matProperties[i]);
        }
    }

    public void setColors(Color color) {

        for (int i = 0; i < _matProperties.Length; i++) {
            _matProperties[i].SetColor("_Color", color);
            renderers[i].SetPropertyBlock(_matProperties[i]);
        }
    }
}

public class SkinRecolorController : MonoBehaviour {

    [SerializeField]
    private RecolorGroup[] recolorGroups;

    [Space, SerializeField]
    private Recolor[] recolors;
    public Recolor[] Recolors => recolors;

    public int currentColor { get; private set; } = -1;

    void Awake() {

        if (recolorGroups == null)
            return;

        for (int i = 0; i < recolorGroups.Length; i++) {
            recolorGroups[i].fillPropertieBlocks();
        }
    }

    public void SetRecolor(Color[] colors) {

        for (int i = 0; i < recolorGroups.Length; i++) {

            if (i >= colors.Length) {
                continue;
            }

            recolorGroups[i].setColors(colors[i]);
        }
    }

    public void SetRecolor(int i) {

        if (recolors[i].colors.Length < recolorGroups.Length) {
            Debug.LogError("not enough colors for this recolor");
        }
        SetRecolor(recolors[i].colors);
        currentColor = i;
    }

    public void SetRecolor(string name) {

        for (int i = 0; i < recolors.Length; i++) {

            if (recolors[i].recolorName == name) {
                SetRecolor(i);
                return;
            }
        }
    }
}
