﻿using System;
using System.Collections.Generic;
using System.Linq;
using AntDesign.Core.Reflection;
using AntDesign.Forms;
using AntDesign.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;

namespace AntDesign
{
    public partial class FormItem : AntDomComponentBase, IFormItem
    {
        private static readonly Dictionary<string, object> _noneColAttributes = new Dictionary<string, object>();

        private readonly string _prefixCls = "ant-form-item";

        [CascadingParameter(Name = "Form")]
        private IForm Form { get; set; }

        [CascadingParameter(Name = "FormItem")]
        private IFormItem ParentFormItem { get; set; }

        [CascadingParameter]
        private EditContext CurrentEditContext { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public RenderFragment LabelTemplate { get; set; }

        [Parameter]
        public ColLayoutParam LabelCol { get; set; }

        [Parameter]
        public AntLabelAlignType? LabelAlign { get; set; }

        [Parameter]
        public OneOf<string, int> LabelColSpan
        {
            get { return LabelCol?.Span ?? null; }
            set
            {
                if (LabelCol == null) LabelCol = new ColLayoutParam();
                LabelCol.Span = value;
            }
        }

        [Parameter]
        public OneOf<string, int> LabelColOffset
        {
            get { return LabelCol?.Offset ?? null; }
            set
            {
                if (LabelCol == null) LabelCol = new ColLayoutParam();
                LabelCol.Offset = value;
            }
        }

        [Parameter]
        public ColLayoutParam WrapperCol { get; set; }

        [Parameter]
        public OneOf<string, int> WrapperColSpan
        {
            get { return WrapperCol?.Span ?? null; }
            set
            {
                if (WrapperCol == null) WrapperCol = new ColLayoutParam();
                WrapperCol.Span = value;
            }
        }

        [Parameter]
        public OneOf<string, int> WrapperColOffset
        {
            get { return WrapperCol?.Offset ?? null; }
            set
            {
                if (WrapperCol == null) WrapperCol = new ColLayoutParam();
                WrapperCol.Offset = value;
            }
        }

        [Parameter]
        public bool NoStyle { get; set; } = false;

        [Parameter]
        public bool Required { get; set; } = false;

        private EditContext EditContext => Form?.EditContext;

        private bool _isValid = true;

        private string _labelCls = "";

        private IControlValueAccessor _control;

        private RenderFragment _formValidationMessages;

        private PropertyReflector _propertyReflector;

        private ClassMapper _labelClassMapper = new ClassMapper();
        private AntLabelAlignType? FormLabelAlign => LabelAlign ?? Form.LabelAlign;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (Form == null)
            {
                throw new InvalidOperationException("Form is null.FormItem should be childContent of Form.");
            }

            SetClass();

            Form.AddFormItem(this);
        }

        protected void SetClass()
        {
            this.ClassMapper
                .Add(_prefixCls)
                .If($"{_prefixCls}-with-help {_prefixCls}-has-error", () => _isValid == false)
                .If($"{_prefixCls}-rtl", () => RTL)
               ;

            _labelClassMapper
                .Add($"{_prefixCls}-label")
                .If($"{_prefixCls}-label-left", () => FormLabelAlign == AntLabelAlignType.Left)
                ;
        }

        private Dictionary<string, object> GetLabelColAttributes()
        {
            if (NoStyle || ParentFormItem != null)
            {
                return _noneColAttributes;
            }

            ColLayoutParam labelColParameter;

            if (LabelCol != null)
            {
                labelColParameter = LabelCol;
            }
            else if (Form.LabelCol != null)
            {
                labelColParameter = Form.LabelCol;
            }
            else
            {
                labelColParameter = new ColLayoutParam();
            }

            return labelColParameter.ToAttributes();
        }

        private Dictionary<string, object> GetWrapperColAttributes()
        {
            if (NoStyle || ParentFormItem != null)
            {
                return _noneColAttributes;
            }

            ColLayoutParam wrapperColParameter;

            if (WrapperCol != null)
            {
                wrapperColParameter = WrapperCol;
            }
            else if (Form.WrapperCol != null)
            {
                wrapperColParameter = Form.WrapperCol;
            }
            else
            {
                wrapperColParameter = new ColLayoutParam();
            }

            return wrapperColParameter.ToAttributes();
        }

        private string GetLabelClass()
        {
            return Required ? $"{_prefixCls}-required" : _labelCls;
        }

        void IFormItem.AddControl<TValue>(AntInputComponentBase<TValue> control)
        {
            if (control.FieldIdentifier.Model == null)
            {
                throw new InvalidOperationException($"Please use @bind-Value in the control with generic type `{typeof(TValue)}`.");
            }

            this._control = control;

            CurrentEditContext.OnValidationStateChanged += (s, e) =>
            {
                control.ValidationMessages = CurrentEditContext.GetValidationMessages(control.FieldIdentifier).Distinct().ToArray();
                this._isValid = !control.ValidationMessages.Any();

                StateHasChanged();
            };

            _formValidationMessages = builder =>
            {
                var i = 0;
                builder.OpenComponent<FormValidationMessage<TValue>>(i++);
                builder.AddAttribute(i++, "Control", control);
                builder.CloseComponent();
            };

            _propertyReflector = PropertyReflector.Create(control.ValueExpression);

            if (_propertyReflector.RequiredAttribute != null)
            {
                _labelCls = $"{_prefixCls}-required";
            }
        }
    }
}
