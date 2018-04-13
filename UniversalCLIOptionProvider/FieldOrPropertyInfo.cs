using System;
using System.Reflection;

namespace UniversalCLIOptionProvider {
	public class FieldOrPropertyInfo {
		private readonly FieldInfo _fieldInfo;
		private readonly PropertyInfo _propertyInfo;
		private readonly bool field;

		private FieldOrPropertyInfo() { }

		public FieldOrPropertyInfo(PropertyInfo source) => _propertyInfo = source;

		public FieldOrPropertyInfo(FieldInfo source) {
			_fieldInfo = source;
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

		public void Setvalue(object reference, object value) {
			if (field) {
				_fieldInfo.SetValue(reference, value);
			}
			else {
				_propertyInfo.SetValue(reference, value);
			}
		}

		public void GetName() {
			_fieldInfo.Name
		}

		public static explicit operator PropertyInfo(FieldOrPropertyInfo src) => src._propertyInfo;
		public static explicit operator FieldInfo(FieldOrPropertyInfo src) => src._fieldInfo;
		public static explicit operator FieldOrPropertyInfo(FieldInfo src) => new FieldOrPropertyInfo(src);
		public static explicit operator FieldOrPropertyInfo(PropertyInfo src) => new FieldOrPropertyInfo(src);
	}
}