using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Linq;
using System.Reflection;

namespace SlidyKitty.Code.Physics;

public class PhysicsService : IDisposable
{
    public World World => _world;

    private float _displayUnitsToSimUnitsRatio = 100f;
    private bool _disposedValue;
    private float _simUnitsToDisplayUnitsRatio = 1 / 100f;
    private World _world;

    public PhysicsService() => _world = CreateWorld();

    /// <summary>
    /// Resets the physics world by disposing of the existing world and creating a new one. This 
    /// is useful for scenarios where you want to start fresh without any existing bodies or forces 
    /// in the world, such as when restarting a game level or clearing the world after a significant event.
    /// </summary>
    public void ResetWorld()
    {
        // Dispose old world and all bodies
        DisposeWorld();

        // Create a completely fresh world
        _world = CreateWorld();
    }

    /// <summary>
    /// Set the ratio of display units (pixels) to simulation units. This is used to convert between the 
    /// two coordinate systems, as the physics engine operates in simulation units while the game rendering 
    /// operates in display units.
    /// </summary>
    /// <param name="displayUnitsPerSimUnit">Number of display units (pixels) per sim unit (metre).</param>
    public void SetDisplayUnitToSimUnitRatio(float displayUnitsPerSimUnit)
    {
        _displayUnitsToSimUnitsRatio = displayUnitsPerSimUnit;
        _simUnitsToDisplayUnitsRatio = 1 / displayUnitsPerSimUnit;
    }

    /// <summary>
    /// Converts a Vector2 value from simulation units to display units using the current ratio.
    /// </summary>
    /// <param name="simUnits">Simulation unit value to convert.</param>
    /// <returns>Number of display units.</returns>
    public Vector2 ToDisplayUnits(Vector2 simUnits) => simUnits * _displayUnitsToSimUnitsRatio;

    /// <summary>
    /// Converts an int value from display units to simulation units using the current ratio.
    /// </summary>
    /// <param name="displayUnits">Display unit value to convert.</param>
    /// <returns>Number of simulation units.</returns>
    public float ToSimUnits(int displayUnits) => displayUnits * _simUnitsToDisplayUnitsRatio;

    /// <summary>
    /// Converts a Vector2 value from display units to simulation units using the current ratio.
    /// </summary>
    /// <param name="displayUnits">Display unit value to convert.</param>
    /// <returns>Number of simulation units.</returns>
    public Vector2 ToSimUnits(Vector2 displayUnits) => displayUnits * _simUnitsToDisplayUnitsRatio;

    /// <summary>
    /// Creates a new physics world with zero gravity. The world is initialized with no bodies or forces.
    /// </summary>
    /// <returns>A physics world, with gravity set to zero.</returns>
    private static World CreateWorld()
    {
        return new World(Vector2.Zero);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
                DisposeWorld();

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Cleanly disposes of the physics world by removing all bodies and unsubscribing any event 
    /// handlers. This ensures that no lingering references to bodies or events remain, which could 
    /// cause memory leaks or unintended side effects.
    /// </summary>
    private void DisposeWorld()
    {
        // If the world is null, there's nothing to dispose or clear, so we can simply return.
        if (_world is null)
            return;

        // Now remove all bodies from the world. We use ToArray() to
        // avoid modifying the collection while iterating.
        foreach (var item in _world.BodyList.ToArray())
            _world.Remove(item);

        // Unsubscribe any existing event handlers to prevent memory leaks or unintended behavior
        UnsubscribeEvent(_world.ContactManager, nameof(ContactManager.BeginContact));
        UnsubscribeEvent(_world.ContactManager, nameof(ContactManager.EndContact));
        UnsubscribeEvent(_world.ContactManager, nameof(ContactManager.PreSolve));
        UnsubscribeEvent(_world.ContactManager, nameof(ContactManager.PostSolve));

        // Clear any remaining event handlers to ensure a clean slate
        _world.ContactManager.BeginContact = null;
        _world.ContactManager.EndContact = null;
        _world.ContactManager.PreSolve = null;
        _world.ContactManager.PostSolve = null;

        // Clear any forces and reset the world state to ensure it's ready for reuse
        _world.ClearForces();
        _world.Clear();
    }

    /// <summary>
    /// Helper method to unsubscribe all event handlers from a specified event on an object.
    /// </summary>
    /// <param name="obj">Parent object.</param>
    /// <param name="eventName">Name of the event to unsubscribe.</param>
    private static void UnsubscribeEvent(object obj, string eventName)
    {
        var eventInfo = obj.GetType().GetEvent(eventName);

        if (eventInfo is null)
            return;

        // Get the hidden field where the event keeps its delegate
        var field = obj.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
            return;

        if (field.GetValue(obj) is not Delegate eventDelegate)
            return;

        foreach (var handler in eventDelegate.GetInvocationList())
        {
            eventInfo.RemoveEventHandler(obj, handler);
        }
    }
}
