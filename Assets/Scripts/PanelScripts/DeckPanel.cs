using UnityEngine;

public class DeckPanel : Panel
{
   public static DeckPanel Instance;

   protected override void Awake()
   {
      base.Awake();
      if (Instance == null) Instance = this;
   }
}
