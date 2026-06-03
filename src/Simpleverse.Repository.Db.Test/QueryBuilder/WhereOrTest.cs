using System;
using Xunit;

namespace Simpleverse.Repository.Db.Test.QueryBuilder
{
	public class WhereOrTest : QueryBuilderTestFixture
	{
		[Fact]
		public void WhereOr_TwoConditions_GeneratesOrClause()
			=> Test<Model>(
				builder => builder.WhereOr(or =>
				{
					or.Where(x => x.String, "abc");
					or.Where(x => x.Int, (int?)1);
				}),
				"WHERE ([String] = @String OR [Int] = @Int)",
				new[] { "String", "Int" }
			);

		[Fact]
		public void WhereOr_OneConditionNullSkipped_GeneratesSimpleClause()
			=> Test<Model>(
				builder => builder.WhereOr(or =>
				{
					or.Where(x => x.String, "abc");
					or.Where(x => x.Int, (int?)null);
				}),
				"WHERE [String] = @String",
				new[] { "String" }
			);

		[Fact]
		public void WhereOr_AllNullConditions_NoWhereAdded()
			=> Test<Model>(
				builder => builder.WhereOr(or =>
				{
					or.Where(x => x.String, (string)null);
					or.Where(x => x.Int, (int?)null);
				}),
				"",
				Array.Empty<string>()
			);

		[Fact]
		public void WhereOr_StringAndBool_GeneratesOrClause()
			=> Test<Model>(
				builder => builder.WhereOr(or =>
				{
					or.Where(x => x.String, "abc");
					or.Where(x => x.Bool, (bool?)true);
				}),
				"WHERE ([String] = @String OR [Bool] = @Bool)",
				new[] { "String", "Bool" }
			);

		[Fact]
		public void WhereOr_NullStringWithBoolTrue_GeneratesSimpleClause()
			=> Test<Model>(
				builder => builder.WhereOr(or =>
				{
					or.Where(x => x.String, (string)null);
					or.Where(x => x.Bool, (bool?)true);
				}),
				"WHERE [Bool] = @Bool",
				new[] { "Bool" }
			);

		[Fact]
		public void WhereOr_BoolFalse_IsNotSkipped()
			=> Test<Model>(
				builder => builder.WhereOr(or =>
				{
					or.Where(x => x.String, "abc");
					or.Where(x => x.Bool, (bool?)false);
				}),
				"WHERE ([String] = @String OR [Bool] = @Bool)",
				new[] { "String", "Bool" }
			);

		[Fact]
		public void WhereOr_CombinedWithWhere_GeneratesAndOrClause()
			=> Test<Model>(
				builder =>
				{
					builder.WhereOr(or =>
					{
						or.Where(x => x.String, "abc");
						or.Where(x => x.Bool, true);
					});
					builder.Where(x => x.Int, (int?)1);
				},
				"WHERE ([String] = @String OR [Bool] = @Bool) AND [Int] = @Int",
				new[] { "String", "Bool", "Int" }
			);
	}
}
