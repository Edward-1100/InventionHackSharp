using System.Collections.Generic;

namespace EntityComponentSystemCSharp.Systems
{
	public class BehaviorRegistry
	{
		Dictionary<string, IAICondition> _conditions = new Dictionary<string, IAICondition>();
		Dictionary<string, IMonsterBehavior> _behaviors = new Dictionary<string, IMonsterBehavior>();

		public void RegisterCondition(IAICondition condition) => _conditions[condition.Name] = condition;
		public void RegisterBehavior(IMonsterBehavior behavior) => _behaviors[behavior.Name] = behavior;
		public IAICondition GetCondition(string name) => _conditions[name];
		public IMonsterBehavior GetBehavior(string name) => _behaviors[name];

		public static BehaviorRegistry CreateDefault()
		{
			var registry = new BehaviorRegistry();
			registry.RegisterCondition(new DefaultCondition());
			registry.RegisterCondition(new NearPlayerCondition());
			registry.RegisterCondition(new NearObjectiveCondition());
			registry.RegisterCondition(new LowHealthCondition());
			registry.RegisterBehavior(new WanderBehavior());
			registry.RegisterBehavior(new AttackBehavior());
			registry.RegisterBehavior(new GuardBehavior());
			registry.RegisterBehavior(new FleeBehavior());
			return registry;
		}
	}
}
