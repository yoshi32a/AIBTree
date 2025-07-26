using UnityEngine;

namespace ArcBT.Actions
{
    public interface IInteractable
    {
        void OnInteract(GameObject interactor);
    }
}