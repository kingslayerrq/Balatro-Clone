using UnityEngine;

[CreateAssetMenu(fileName = "HandTypeConfig", menuName = "Scriptable Objects/HandTypeConfig")]
public class HandTypeConfig : ScriptableObject
{
    public Enums.BasePokerHandType type = Enums.BasePokerHandType.None;
    public float baseChips;
    public float baseMults;
    public int lvl = 1;
    public float chipsOnLvlUp;
    public float multsOnLvlUp = 1f;

    public HandType Create()
    {
        return new HandType
        {
            type = type,
            baseChips = baseChips,
            baseMults = baseMults,
            lvl = lvl,
            chipsOnLvlUp = chipsOnLvlUp,
            multsOnLvlUp = multsOnLvlUp
        };
    }

    public class HandType
    {
        public Enums.BasePokerHandType type;
        public float baseChips;
        public float baseMults;
        public int lvl;
        public float chipsOnLvlUp;
        public float multsOnLvlUp;

        public void LevelUp()
        {
            this.lvl += 1;
            this.baseChips += chipsOnLvlUp;
            this.baseMults += multsOnLvlUp;
        }
    }
}
