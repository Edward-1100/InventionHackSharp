using NUnit.Framework;
using EntityComponentSystemCSharp.Components;
using EntityComponentSystemCSharp.Systems;

namespace EntityComponentSystemCSharp
{
    [TestFixture]
    public class HealthSystemTests
    {
        MockEngine _mockEngine;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _mockEngine = new MockEngine(new EntityManager(), new MockLogger(), new MockMap());
        }

        [Test]
        public void EntityDiesWhenHealthZero()
        {
            var em = new EntityManager();
            var entity = em.CreateEntity();

            entity.AddComponent(new Life() {Health = 0, MaxHealth = 10});
            entity.AddComponent(new Actor());
            entity.AddComponent(new Name() {NameString = "TestOrc"});
            entity.AddComponent(new Glyph() {glyph = 42});

            var healthSystem = new HealthSystem(_mockEngine);

            healthSystem.Run(entity);

            Assert.IsFalse(entity.HasComponent<Actor>(), "Should not have an Actor component.");
            Assert.IsFalse(entity.HasComponent<Life>(), "Should not have a Life component.");
            Assert.IsTrue(entity.HasComponent<Dead>(), "Should have a Dead component.");

            var glyph = entity.GetComponent<Glyph>();
            Assert.AreEqual(636, glyph.glyph, "glyph should be changed to corpse.");
        }
    }
}