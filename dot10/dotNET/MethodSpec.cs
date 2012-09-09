﻿using System;
using dot10.dotNET.MD;

namespace dot10.dotNET {
	/// <summary>
	/// A high-level representation of a row in the MethodSpec table
	/// </summary>
	public abstract class MethodSpec : IHasCustomAttribute {
		/// <summary>
		/// The row id in its table
		/// </summary>
		protected uint rid;

		/// <inheritdoc/>
		public MDToken MDToken {
			get { return new MDToken(Table.MethodSpec, rid); }
		}

		/// <inheritdoc/>
		public int HasCustomAttributeTag {
			get { return 21; }
		}

		/// <summary>
		/// From column MethodSpec.Method
		/// </summary>
		public abstract IMethodDefOrRef Method { get; set; }

		/// <summary>
		/// From column MethodSpec.Instantiation
		/// </summary>
		public abstract CallingConventionSig Instantiation { get; set; }

		/// <summary>
		/// Gets/sets the generic instance method sig
		/// </summary>
		public GenericInstMethodSig GenericInstMethodSig {
			get { return Instantiation as GenericInstMethodSig; }
			set { Instantiation = value; }
		}
	}

	/// <summary>
	/// A MethodSpec row created by the user and not present in the original .NET file
	/// </summary>
	public class MethodSpecUser : MethodSpec {
		IMethodDefOrRef method;
		CallingConventionSig instantiation;

		/// <inheritdoc/>
		public override IMethodDefOrRef Method {
			get { return method; }
			set { method = value; }
		}

		/// <inheritdoc/>
		public override CallingConventionSig Instantiation {
			get { return instantiation; }
			set { instantiation = value; }
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public MethodSpecUser() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="method">The generic method</param>
		public MethodSpecUser(IMethodDefOrRef method)
			: this(method, null) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="method">The generic method</param>
		/// <param name="sig">The instantiated method sig</param>
		public MethodSpecUser(IMethodDefOrRef method, GenericInstMethodSig sig) {
			this.method = method;
			this.instantiation = sig;
		}
	}

	/// <summary>
	/// Created from a row in the MethodSpec table
	/// </summary>
	sealed class MethodSpecMD : MethodSpec {
		/// <summary>The module where this instance is located</summary>
		ModuleDefMD readerModule;
		/// <summary>The raw table row. It's null until <see cref="InitializeRawRow"/> is called</summary>
		RawMethodSpecRow rawRow;

		UserValue<IMethodDefOrRef> method;
		UserValue<CallingConventionSig> instantiation;

		/// <inheritdoc/>
		public override IMethodDefOrRef Method {
			get { return method.Value; }
			set { method.Value = value; }
		}

		/// <inheritdoc/>
		public override CallingConventionSig Instantiation {
			get { return instantiation.Value; }
			set { instantiation.Value = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerModule">The module which contains this <c>MethodSpec</c> row</param>
		/// <param name="rid">Row ID</param>
		/// <exception cref="ArgumentNullException">If <paramref name="readerModule"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">If <paramref name="rid"/> is <c>0</c> or &gt; <c>0x00FFFFFF</c></exception>
		public MethodSpecMD(ModuleDefMD readerModule, uint rid) {
#if DEBUG
			if (readerModule == null)
				throw new ArgumentNullException("readerModule");
			if (rid == 0 || rid > 0x00FFFFFF)
				throw new ArgumentException("rid");
			if (readerModule.TablesStream.Get(Table.MethodSpec).Rows < rid)
				throw new BadImageFormatException(string.Format("MethodSpec rid {0} does not exist", rid));
#endif
			this.rid = rid;
			this.readerModule = readerModule;
			Initialize();
		}

		void Initialize() {
			method.ReadOriginalValue = () => {
				InitializeRawRow();
				return readerModule.ResolveMethodDefOrRef(rawRow.Method);
			};
			instantiation.ReadOriginalValue = () => {
				InitializeRawRow();
				return readerModule.ReadSignature(rawRow.Instantiation);
			};
		}

		void InitializeRawRow() {
			if (rawRow != null)
				return;
			rawRow = readerModule.TablesStream.ReadMethodSpecRow(rid);
		}
	}
}
