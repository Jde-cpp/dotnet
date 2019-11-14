using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Jde.DB
{
	public sealed class Transaction : IDisposable, IDataSource
	{
		#region Constructors
		public Transaction()
		{}
		public Transaction( Database db )
		{
			Database = db ?? throw new ArgumentNullException( "db" );
		}
		#endregion
		#region IDisposable Members
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize(this);
		}
		void Dispose( bool disposing )
		{
			if( disposing )
			{
				Rollback();
				DbConnection?.Dispose();
			}
		}
		#endregion
		#region Execute
		public int Execute( string sql )
		{
			return Database.Execute( sql, GetTransaction() );
		}

		//public int Execute( Command command )
		//{
		//   AddCommand( command );
		//   return DataSource.Execute( command, GetTransaction() );
		//}
		//public int Execute( ICollection<Command> commands )
		//{
		//   AddCommands( commands );
		//   return DataSource.Execute( commands, GetTransaction() );
		//}

		/// <exception cref="DBException"/>
		public long? ExecuteCount( Command command )
		{
			AddCommand( command );
			//DBLogging.CommandExecuting( command );
			var count = command.Database.ExecuteScalarCount( command.GetDBCommand(this), GetTransaction() ) as long?;

			return count;
		}

		/// <exception cref="DBException"/>
		public int ExecuteNonQuery( Command command )
		{
			AddCommand( command );
			//DBLogging.CommandExecuting( command );
			int count = command.Database.ExecuteNonQuery( command.GetDBCommand(this), GetTransaction() );
			command.SetDataTypeId();

			return count;
		}
		/// <exception cref="DBException"/>
		public int ExecuteNonQuery( ICollection<Command> commands )
		{
			int count = 0;
			foreach( var command in commands )
				count += ExecuteNonQuery( command );
			return count;
		}
		#endregion
		#region Commands
		//LinkedList<Command> _commands;
		LinkedList<Command> _commandExs;
		LinkedList<Command> CommandExs
		{
			get{ return _commandExs; }
			set{ _commandExs = value; }
		}
		//LinkedList<Command> Commands
		//{
		//   get{ return _commands; }
		//   set{ _commands = value; }
		//}
		void AddCommand( Command command )
		{
			if( command.OnCommit!=null )
			{
				if( CommandExs==null )
					CommandExs = new LinkedList<Command>();

				CommandExs.AddLast( command );
			}
		}

		//void AddCommand( Command command )
		//{
		//   if( command.DataType!=null )
		//   {
		//      if( Commands==null )
		//         Commands = new LinkedList<Command>();

		//      Commands.AddLast( command );
		//   }
		//}
		//void AddCommands( ICollection<Command> commands )
		//{
		//   foreach( Command command in commands )
		//      AddCommand( command );
		//}
		void CommitCommands()
		{
		//   if( Commands!=null )
		//   {
		//      foreach( Command command in Commands )
		//      {
		//         if( command.DataType!=null )
		//            command.DataType.Commit();
		//      }
		//   }
		   if( CommandExs!=null )
		   {
		      foreach( var command in CommandExs )
		         command.Commit();
		   }
		}
		#endregion
		#region Commit
		public void Commit()
		{
			if( _transaction!=null )
			{
				try
				{
					_transaction.Commit();
					_transaction.Connection.Dispose();
					_transaction=null;
				}
				catch( Exception  )
				{
					//Logger.Write( e.ToString(), "error commiting the transaction." );
					throw;
				}
				CommitCommands();
			}
		}
		#endregion
		#region ExecuteReader
		public IDataReader ExecuteReader( string sql )
		{
			return Database.ExecuteReader( sql, CommandType.Text, GetTransaction() );
//				Database.ExecuteReader( t, CommandType.Text, sql );
		}
		public IDataReader ExecuteReader( DbCommand command )
		{
			command.Transaction = GetTransaction();
			command.Connection = command.Transaction.Connection;
			return command.ExecuteReader();
//				Database.ExecuteReader( t, CommandType.Text, sql );
		}
		#endregion
		#region LoadDataSet
		public DataSet LoadDataSet( string sql )
		{
			return Database.LoadDataSet( sql, GetTransaction() );
		}
		#endregion
		#region LoadDataTable
		public DataTable LoadDataTable( string sql )
		{
			return Database.LoadDataTable( sql, GetTransaction() );
		}
		#endregion
		#region DataSource
		DbConnection DbConnection{ get; set;}
		public Database Database{get;}
		SqlSyntax IDataSource.Syntax
		{
			get{ return Database.Syntax; }
		}
		#endregion
		#region Rollback
		public void Rollback()
		{
			if( _transaction!=null )
			{
				try
				{
					_transaction.Rollback();
					if( _transaction.Connection!=null )
						_transaction.Connection.Close();
					_transaction.Dispose();
					_transaction=null;
				}
				catch( Exception  )
				{
					//Logger.Write( e.ToString(), "error" );
					throw;
				}
			}
		}
		#endregion
		#region StartTime
		DateTime? _startTime;
		/// <summary>Can't return null now, future to use db default for start time.</summary>
		public DateTime? StartTime
		{
			get
			{
				if( _startTime==null )
					_startTime = DateTime.UtcNow;
				return _startTime.Value;
			}
		}
		#endregion
		#region Transaction
		DbTransaction _transaction;
		DbTransaction GetTransaction()
		{
			if( _transaction==null )
			{
/*				if( Database==null )
				{
					DbConnection = Command.DefaultDatabase.CreateConnection();
					DbConnection.Open();
					_transaction = DbConnection.BeginTransaction();
				}
				else*/
					_transaction = Database.BeginTransaction();
			}
			return _transaction;
		}
		#endregion
	}
}
