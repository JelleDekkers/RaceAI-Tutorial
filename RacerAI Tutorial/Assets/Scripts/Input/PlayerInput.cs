using UnityEngine;

namespace RacerAI.Input
{
    public class PlayerInput : ICarInput
    {
        private Vector2 input = new Vector2();

        public Vector2 GetInput()
        {
            input.x = UnityEngine.Input.GetAxisRaw("Horizontal");
            input.y = UnityEngine.Input.GetAxisRaw("Vertical");

            return input;
        }
    }
}