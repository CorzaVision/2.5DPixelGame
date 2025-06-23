using UnityEngine;

namespace Interface
{
    /// <summary>
    /// Interface for objects that can take damage in the game.
    /// Implement this interface on any GameObject that should be able to receive damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Called when the object takes damage.
        /// </summary>
        /// <param name="damage">The amount of damage to apply.</param>
        void TakeDamage(float damage);
    }
}
