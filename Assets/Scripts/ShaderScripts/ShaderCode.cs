using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the card shader of one Image: per-card material instance, edition keyword,
/// and the tilt-based rotation the edition effects react to.
/// </summary>
public class ShaderCode : MonoBehaviour
{
    private static readonly int RotationId = Shader.PropertyToID("_Rotation");
    // "_EDITION_REGULAR", ... built once instead of on every call
    private static string[] _editionKeywords;

    Image image;
    Material m;
    private Sprite _prevSprite;
    private Vector2 _prevRotation = new Vector2(float.NaN, float.NaN);
    // edition requested before Start created the material
    private int _edition = 0;

    private static string[] EditionKeywords
    {
        get
        {
            if (_editionKeywords != null) return _editionKeywords;
            _editionKeywords = new string[Constants.CARD_EDITIONS.Length];
            for (int i = 0; i < _editionKeywords.Length; i++)
            {
                _editionKeywords[i] = "_EDITION_" + Constants.CARD_EDITIONS[i];
            }
            return _editionKeywords;
        }
    }

    void Start()
    {
        image = GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("No Image component found on GameObject: " + gameObject.name);
            enabled = false;
            return;
        }

        if (image.material == null)
        {
            enabled = false;
            return;
        }

        // Create a unique material instance for each card
        m = Instantiate(image.material);
        image.material = m;

        // Apply the sprite's texture
        if (image.sprite != null)
        {
            _prevSprite = image.sprite;
            m.mainTexture = image.sprite.texture;
        }

        ApplyEdition();
    }

    private void OnDestroy()
    {
        // the instance created in Start is owned by this component
        if (m != null) Destroy(m);
    }

    void Update()
    {
        // Update Material texture
        if (_prevSprite != null && _prevSprite != image.sprite)
        {
            _prevSprite = image.sprite;
            if (_prevSprite != null) m.mainTexture = _prevSprite.texture;
            image.SetMaterialDirty();
        }

        Vector3 eulerAngles = transform.parent.localRotation.eulerAngles;

        // Keep the angles within the range of -90 to 90 degrees
        float xAngle = ClampAngle(eulerAngles.x, -90f, 90f);
        float yAngle = ClampAngle(eulerAngles.y, -90f, 90f);

        var rotation = new Vector2(ExtensionMethods.Remap(xAngle, -20, 20, -.5f, .5f),
            ExtensionMethods.Remap(yAngle, -20, 20, -.5f, .5f));
        // cards lying still (deck, used pile) do not need a material update every frame
        if (rotation == _prevRotation) return;
        _prevRotation = rotation;
        m.SetVector(RotationId, rotation);
    }

    // Method to clamp an angle between a minimum and maximum value
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -180f)
            angle += 360f;
        if (angle > 180f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// Set the Edition for the card's shader
    /// </summary>
    /// <param name="index">Index into Constants.CARD_EDITIONS</param>
    public void SetEdition(int index)
    {
        _edition = Mathf.Clamp(index, 0, EditionKeywords.Length - 1);
        // before Start there is no material yet: Start applies the stored edition
        if (m != null) ApplyEdition();
    }

    private void ApplyEdition()
    {
        var keywords = EditionKeywords;
        for (int i = 0; i < keywords.Length; i++)
        {
            if (i == _edition) m.EnableKeyword(keywords[i]);
            else m.DisableKeyword(keywords[i]);
        }

        // Force material update
        image.SetMaterialDirty();
    }
}
