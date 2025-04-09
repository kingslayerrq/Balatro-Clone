using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(UnityEngine.UIElements.Image))]
public class UIImageAnimation : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[21];
    public int spritePerFrame = 6;
    public bool loop = true;
    public bool isSet = false;
    public bool destroyOnEnd = false;

    private int index = 0;
    private Image image;
    private int frame = 0;

    void Awake() {
        image = GetComponent<Image> ();
    }

    void Update () {
        if (!isSet) return;
        if (!loop && index == sprites.Length) return;
        frame ++;
        if (frame < spritePerFrame) return;
        image.sprite = sprites [index];
        frame = 0;
        index ++;
        if (index >= sprites.Length) {
            if (loop) index = 0;
            if (destroyOnEnd) Destroy (gameObject);
        }
    }
}
