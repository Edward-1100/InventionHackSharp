using System.Collections.Generic;
using EntityComponentSystemCSharp.Components;
using MegaDungeon.Contracts;

namespace EntityComponentSystemCSharp.Systems
{
	public class AISystem : SystemBase, ISystem
	{
		BehaviorRegistry _registry;
		List<AIRule> _defaultRules;

		public AISystem(IEngine engine, BehaviorRegistry registry, List<AIRule> defaultRules) : base(engine)
		{
			_registry = registry;
			_defaultRules = defaultRules;
		}

		public override void Run(EntityManager.Entity entity)
		{
			if(!entity.HasComponent<WanderingMonster>()) {return;}
			if(!entity.HasComponent<Location>()) {return;}

			var profile = entity.GetComponent<AIProfile>();
			var rules = (profile != null && profile.Rules.Count > 0) ? profile.Rules : _defaultRules;

			foreach(var rule in rules)
			{
				var condition = _registry.GetCondition(rule.Condition);
				if(condition.Matches(_engine, entity))
				{
					var behavior = _registry.GetBehavior(rule.Behavior);
					behavior.Act(_engine, entity);
					return;
				}
			}
		}
	}
}
