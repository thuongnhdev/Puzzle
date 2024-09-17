using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextWriter : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI txtWriter;
    private float timePerChar = 0.1f;
    private string text;
    private float timer = 0;
    private int currentCharacter = 0;
    private bool canRun = false;
    public void SetText(string _textData, float _timePerChar)
    {
        currentCharacter = 0;
        timer = 0;
        FadeSpeed = _timePerChar;
        text = _textData;
        txtWriter.enabled = false;
        txtWriter.text = text.Replace("\\n", "\n");
        StartAnim();

    }

    public void SetTimePerChar(float _timePerChar)
    {
        FadeSpeed = _timePerChar;
    }

    public void StartAnim()
    {
        canRun = true;
    }

    public void StopAnim()
    {
        canRun = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (txtWriter != null && canRun)
        {
            StopAnim();
            txtWriter.enabled = true;
            StartCoroutine(FadeInText());
        }
    }

    [SerializeField] private float FadeSpeed = 20.0f;
    [SerializeField] private int RolloverCharacterSpread = 10;
    /// <summary>
    /// Method to animate (fade in) vertex colors of a TMP Text object.
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeInText()
    {
        // Set the whole text transparent
        txtWriter.color = new Color
            (
                txtWriter.color.r,
                txtWriter.color.g,
                txtWriter.color.b,
                0
            );
        // Need to force the text object to be generated so we have valid data to work with right from the start.
        txtWriter.ForceMeshUpdate();


        TMP_TextInfo textInfo = txtWriter.textInfo;
        Color32[] newVertexColors;

        this.currentCharacter = 0;
        int startingCharacterRange = currentCharacter;
        bool isRangeMax = false;

        while (!isRangeMax)
        {
            int characterCount = textInfo.characterCount;

            // Spread should not exceed the number of characters.
            byte fadeSteps = (byte)Mathf.Max(1, 255 / RolloverCharacterSpread);

            for (int i = startingCharacterRange; i < currentCharacter + 1; i++)
            {
                // Skip characters that are not visible (like white spaces)
                if (!textInfo.characterInfo[i].isVisible) continue;

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the current character's alpha value.
                byte alpha = (byte)Mathf.Clamp(newVertexColors[vertexIndex + 0].a + fadeSteps, 0, 255);

                // Set new alpha values.
                newVertexColors[vertexIndex + 0].a = alpha;
                newVertexColors[vertexIndex + 1].a = alpha;
                newVertexColors[vertexIndex + 2].a = alpha;
                newVertexColors[vertexIndex + 3].a = alpha;

                if (alpha == 255)
                {
                    startingCharacterRange += 1;

                    if (startingCharacterRange == characterCount)
                    {
                        // Update mesh vertex data one last time.
                        txtWriter.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                        yield return new WaitForSeconds(1.0f);

                        // Reset the text object back to original state.
                        txtWriter.ForceMeshUpdate();

                        yield return new WaitForSeconds(1.0f);

                        // Reset our counters.
                        currentCharacter = 0;
                        startingCharacterRange = 0;
                        //isRangeMax = true; // Would end the coroutine.
                    }
                }
            }

            // Upload the changed vertex colors to the Mesh.
            txtWriter.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            if (currentCharacter + 1 < characterCount) currentCharacter += 1;
            else
            {
                txtWriter.color = new Color(txtWriter.color.r, txtWriter.color.g, txtWriter.color.b, 1);
                isRangeMax = true;
            }
            yield return new WaitForSeconds(0.25f - FadeSpeed * 0.01f);
        }
    }

    private IEnumerator FadeInText(float timeSpeed, TextMeshProUGUI text)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        while (text.color.a < 1.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime * timeSpeed));
            yield return null;
        }
    }

    private IEnumerator FadeOutText(float timeSpeed, TextMeshProUGUI text)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        while (text.color.a > 0.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime * timeSpeed));
            yield return null;
        }
    }
}
