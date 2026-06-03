using Dapper;
using Dapper.Contrib.Extensions;
using Simpleverse.Repository.Db.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Simpleverse.Repository.Db.Test.SqlServer.QueryBuilder
{
	[Collection("SqlServerCollection")]
	public class WhereOrIntegrationTest : DatabaseTestFixture
	{
		public WhereOrIntegrationTest(DatabaseFixture fixture, ITestOutputHelper output)
			: base(fixture, output)
		{
		}

		[Fact]
		public async Task WhereOr_TwoConditions_ReturnsRowsMatchingEither()
		{
			using var profiler = Profile();
			using var connection = _fixture.GetProfiledConnection();

			await connection.OpenAsync();
			await connection.TruncateAsync<DataType>();
			await connection.InsertAsync(TestData.DataTypeData(10));

			var queryBuilder = new QueryBuilder<DataType>();
			var template = queryBuilder.AsSelect(
				builder =>
				{
					builder.SelectAll();
					builder.WhereOr(or =>
					{
						or.Where(x => x.Name, "1");
						or.Where(x => x.Enum, @Enum.Even);
					});
				},
				new DbQueryOptions()
			);

			var results = (await connection.QueryAsync<DataType>(template.RawSql, template.Parameters)).ToList();

			// Name="1" has Enum.Odd; Even items are 2,4,6,8,10 — total 6
			Assert.Equal(6, results.Count);
			Assert.Contains(results, x => x.Name == "1");
			Assert.All(results.Where(x => x.Name != "1"), x => Assert.Equal(@Enum.Even, x.Enum));
		}

		[Fact]
		public async Task WhereOr_NullConditionSkipped_FiltersOnlyByNonNullCondition()
		{
			using var profiler = Profile();
			using var connection = _fixture.GetProfiledConnection();

			await connection.OpenAsync();
			await connection.TruncateAsync<DataType>();
			await connection.InsertAsync(TestData.DataTypeData(10));

			var queryBuilder = new QueryBuilder<DataType>();
			var template = queryBuilder.AsSelect(
				builder =>
				{
					builder.SelectAll();
					builder.WhereOr(or =>
					{
						or.Where(x => x.Name, "1");
						or.Where(x => x.Enum, (@Enum?)null);
					});
				},
				new DbQueryOptions()
			);

			var results = (await connection.QueryAsync<DataType>(template.RawSql, template.Parameters)).ToList();

			Assert.Single(results);
			Assert.Equal("1", results[0].Name);
		}

		[Fact]
		public async Task WhereOr_StringAndBoolTrue_ReturnsRowsMatchingEither()
		{
			using var profiler = Profile();
			using var connection = _fixture.GetProfiledConnection();

			await connection.OpenAsync();
			await connection.TruncateAsync<DataType>();
			await connection.InsertAsync(TestData.DataTypeData(10));

			var queryBuilder = new QueryBuilder<DataType>();
			var template = queryBuilder.AsSelect(
				builder =>
				{
					builder.SelectAll();
					builder.WhereOr(or =>
					{
						or.Where(x => x.Name, "1");
						or.Where(x => x.Bool, (bool?)true);
					});
				},
				new DbQueryOptions()
			);

			var results = (await connection.QueryAsync<DataType>(template.RawSql, template.Parameters)).ToList();

			// Name="1" has Bool=false; Bool=true items are 2,4,6,8,10 — total 6
			Assert.Equal(6, results.Count);
			Assert.Contains(results, x => x.Name == "1");
			Assert.All(results.Where(x => x.Name != "1"), x => Assert.True(x.Bool));
		}

		[Fact]
		public async Task WhereOr_NullStringWithBoolTrue_FiltersOnlyByBool()
		{
			using var profiler = Profile();
			using var connection = _fixture.GetProfiledConnection();

			await connection.OpenAsync();
			await connection.TruncateAsync<DataType>();
			await connection.InsertAsync(TestData.DataTypeData(10));

			var queryBuilder = new QueryBuilder<DataType>();
			var template = queryBuilder.AsSelect(
				builder =>
				{
					builder.SelectAll();
					builder.WhereOr(or =>
					{
						or.Where(x => x.Name, (string)null);
						or.Where(x => x.Bool, (bool?)true);
					});
				},
				new DbQueryOptions()
			);

			var results = (await connection.QueryAsync<DataType>(template.RawSql, template.Parameters)).ToList();

			// Only Bool=true items: 2,4,6,8,10 — total 5
			Assert.Equal(5, results.Count);
			Assert.All(results, x => Assert.True(x.Bool));
		}

		[Fact]
		public async Task WhereOr_BoolFalse_IsNotSkipped()
		{
			using var profiler = Profile();
			using var connection = _fixture.GetProfiledConnection();

			await connection.OpenAsync();
			await connection.TruncateAsync<DataType>();
			await connection.InsertAsync(TestData.DataTypeData(10));

			var queryBuilder = new QueryBuilder<DataType>();
			var template = queryBuilder.AsSelect(
				builder =>
				{
					builder.SelectAll();
					builder.WhereOr(or =>
					{
						or.Where(x => x.Name, "2");
						or.Where(x => x.Bool, (bool?)false);
					});
				},
				new DbQueryOptions()
			);

			var results = (await connection.QueryAsync<DataType>(template.RawSql, template.Parameters)).ToList();

			// Name="2" has Bool=true; Bool=false items are 1,3,5,7,9 — total 6
			Assert.Equal(6, results.Count);
			Assert.Contains(results, x => x.Name == "2");
			Assert.All(results.Where(x => x.Name != "2"), x => Assert.False(x.Bool));
		}

		[Fact]
		public async Task WhereOr_AllNullConditions_ReturnsAllRows()
		{
			using var profiler = Profile();
			using var connection = _fixture.GetProfiledConnection();

			await connection.OpenAsync();
			await connection.TruncateAsync<DataType>();
			await connection.InsertAsync(TestData.DataTypeData(10));

			var queryBuilder = new QueryBuilder<DataType>();
			var template = queryBuilder.AsSelect(
				builder =>
				{
					builder.SelectAll();
					builder.WhereOr(or =>
					{
						or.Where(x => x.Name, (string)null);
						or.Where(x => x.Enum, (@Enum?)null);
					});
				},
				new DbQueryOptions()
			);

			var results = (await connection.QueryAsync<DataType>(template.RawSql, template.Parameters)).ToList();

			Assert.Equal(10, results.Count);
		}
	}
}
