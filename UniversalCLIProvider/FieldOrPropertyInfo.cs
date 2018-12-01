using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniversalCLIProvider {
	public static class FieldOrPropertyMethods {
		public static IEnumerable<FieldOrPropertyInfo> GetFieldsAndProperties(this TypeInfo src) => src.DeclaredProperties
			.Select(x => new FieldOrPropertyInfo(x))
			.Concat(src.DeclaredProperties.Select(x => new FieldOrPropertyInfo(x)));
	}

	public class FieldOrPropertyInfo : MemberInfo {
		private readonly FieldInfo _fieldInfo;
		private readonly MemberInfo _memberInfo;
		private readonly PropertyInfo _propertyInfo;
		private readonly bool field;

		public override Type DeclaringType => _memberInfo.DeclaringType;

		public override MemberTypes MemberType => _memberInfo.MemberType;

		public override string Name => _memberInfo.Name;

		public override Type ReflectedType => _memberInfo.ReflectedType;


		public bool CanRead {
			get {
				if (field) {
					return _fieldInfo.IsPublic;
				}
				else {
					if (!_propertyInfo.CanRead) {
						return false;
					}
					else {
						return _propertyInfo.GetGetMethod().IsPublic;
					}
				}
			}
		}

		public bool CanWrite {
			get {
				if (field) {
					return _fieldInfo.IsPublic;
				}
				else {
					if (!_propertyInfo.CanWrite) {
						return false;
					}
					else {
						return _propertyInfo.GetSetMethod().IsPublic;
					}
				}
			}
		}

		private FieldOrPropertyInfo() { }

		public FieldOrPropertyInfo(PropertyInfo source) {
			_propertyInfo = source;
			_memberInfo = source;
		}

		public FieldOrPropertyInfo(FieldInfo source) {
			_fieldInfo = source;
			_memberInfo = source;
			field = true;
		}

		public Type GetType() {
			if (field) {
				return _fieldInfo.GetType();
			}
			else {
				return _propertyInfo.GetType();
			}
		}

		public object GetValue(object reference) {
			if (field) {
				return _fieldInfo.GetValue(reference);
			}
			else {
				return _propertyInfo.GetValue(reference);
			}
		}

		public void SetValue(object reference, object value) {
			if (field) {
				_fieldInfo.SetValue(reference, value);
			}
			else {
				_propertyInfo.SetValue(reference, value);
			}
		}


		public static explicit operator PropertyInfo(FieldOrPropertyInfo src) => src._propertyInfo;
		public static explicit operator FieldInfo(FieldOrPropertyInfo src) => src._fieldInfo;
		public static explicit operator FieldOrPropertyInfo(FieldInfo src) => new FieldOrPropertyInfo(src);
		public static explicit operator FieldOrPropertyInfo(PropertyInfo src) => new FieldOrPropertyInfo(src);
		public override object[] GetCustomAttributes(bool inherit) => _memberInfo.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) =>
			_memberInfo.GetCustomAttributes(attributeType, inherit);

		public override bool IsDefined(Type attributeType, bool inherit) => _memberInfo.IsDefined(attributeType, inherit);
	}
}