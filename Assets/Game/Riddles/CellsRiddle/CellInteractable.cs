using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Game.Riddles.CellsRiddle
{
    public class CellInteractable : XRBaseInteractable
    {
        private GridCell _gridCell;

        protected override void Awake()
        {
            base.Awake();
            _gridCell = GetComponent<GridCell>();
        }
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            _gridCell.ToggleState();
        }
    }
}