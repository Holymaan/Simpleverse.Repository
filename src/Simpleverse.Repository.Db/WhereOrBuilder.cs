using Dapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simpleverse.Repository.Db
{
	public class WhereOrBuilder<TTable>
	{
		private readonly Table<TTable> _table;
		private readonly List<string> _conditions = new List<string>();
		private readonly DynamicParameters _parameters = new DynamicParameters();

		internal WhereOrBuilder(Table<TTable> table)
		{
			_table = table;
		}

		public WhereOrBuilder<TTable> Where<T>(Expression<Func<TTable, T>> column, T? value, string condition = "=") where T : struct
		{
			if (!value.HasValue)
				return this;

			return Add(_table.Column(column), value.Value, condition);
		}

		public WhereOrBuilder<TTable> Where(Expression<Func<TTable, string>> column, string value, string condition = "=")
		{
			if (string.IsNullOrEmpty(value))
				return this;

			return Add(_table.Column(column), value, condition);
		}

		public WhereOrBuilder<TTable> Where<T>(Expression<Func<TTable, T>> column, T value, string condition = "=")
		{
			if (value == null)
				return this;

			return Add(_table.Column(column), value, condition);
		}

		private WhereOrBuilder<TTable> Add<T>(Selector selector, T value, string condition)
		{
			var paramName = DbRepository.ParameterName(selector.Column, selector.TableAlias);
			selector.Is($"@{paramName}", condition: condition);
			_conditions.Add(selector.ToString());
			_parameters.Add(paramName, value);
			return this;
		}

		internal bool HasConditions => _conditions.Count > 0;

		internal (string Sql, DynamicParameters Parameters) Build()
		{
			if (_conditions.Count == 0)
				return (null, null);

			var sql = _conditions.Count == 1
				? _conditions[0]
				: $"({string.Join(" OR ", _conditions)})";

			return (sql, _parameters);
		}
	}
}
