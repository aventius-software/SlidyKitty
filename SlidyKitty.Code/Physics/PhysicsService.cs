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

    public PhysicsService()
    {
        _world = CreateWorld();
    }

    public void ResetWorld()
    {
        // Dispose old world and all bodies
        DisposeWorld();

        // Create a completely fresh world
        _world = CreateWorld();
    }

    public void SetDisplayUnitToSimUnitRatio(float displayUnitsPerSimUnit)
    {
        _displayUnitsToSimUnitsRatio = displayUnitsPerSimUnit;
        _simUnitsToDisplayUnitsRatio = 1 / displayUnitsPerSimUnit;
    }

    public Vector2 ToDisplayUnits(Vector2 simUnits) => simUnits * _displayUnitsToSimUnitsRatio;

    public float ToSimUnits(int displayUnits) => displayUnits * _simUnitsToDisplayUnitsRatio;

    public Vector2 ToSimUnits(Vector2 displayUnits) => displayUnits * _simUnitsToDisplayUnitsRatio;

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
            {
                DisposeWorld();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

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

    private static void UnsubscribeEvent(object obj, string eventName)
    {
        var eventInfo = obj.GetType().GetEvent(eventName);

        if (eventInfo == null)
            return;

        // Get the hidden field where the event keeps its delegate
        var field = obj.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
            return;

        if (field.GetValue(obj) is not Delegate eventDelegate)
            return;

        foreach (var handler in eventDelegate.GetInvocationList())
        {
            eventInfo.RemoveEventHandler(obj, handler);
        }
    }
}
