import React, { useState } from 'react';
import './CreateForm.css';

interface CreateFormProps {
  title: string;
  fields: Array<{
    name: string;
    label: string;
    type: 'text' | 'textarea' | 'select' | 'date';
    required?: boolean;
    options?: Array<{ value: string | number; label: string }>;
  }>;
  onSubmit: (data: Record<string, any>) => Promise<void>;
  onCancel: () => void;
  isOpen: boolean;
}

const CreateForm: React.FC<CreateFormProps> = ({
  title,
  fields,
  onSubmit,
  onCancel,
  isOpen
}) => {
  const [formData, setFormData] = useState<Record<string, any>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (name: string, value: any) => {
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await onSubmit(formData);
      setFormData({});
    } catch (err: any) {
      setError(err.message || 'An error occurred');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="create-form-overlay">
      <div className="create-form-modal">
        <div className="create-form-header">
          <h2 className="create-form-title">{title}</h2>
          <button className="create-form-close" onClick={onCancel}>×</button>
        </div>
        
        <form className="create-form" onSubmit={handleSubmit}>
          {fields.map((field) => (
            <div key={field.name} className="create-form-field">
              <label className="create-form-label" htmlFor={field.name}>
                {field.label}
                {field.required && <span className="required">*</span>}
              </label>
              
              {field.type === 'textarea' ? (
                <textarea
                  id={field.name}
                  name={field.name}
                  value={formData[field.name] || ''}
                  onChange={(e) => handleInputChange(field.name, e.target.value)}
                  required={field.required}
                  className="create-form-textarea"
                  rows={3}
                />
              ) : field.type === 'select' ? (
                <select
                  id={field.name}
                  name={field.name}
                  value={formData[field.name] || ''}
                  onChange={(e) => handleInputChange(field.name, e.target.value)}
                  required={field.required}
                  className="create-form-select"
                >
                  <option value="">Select {field.label}</option>
                  {field.options?.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              ) : (
                <input
                  id={field.name}
                  name={field.name}
                  type={field.type}
                  value={formData[field.name] || ''}
                  onChange={(e) => handleInputChange(field.name, e.target.value)}
                  required={field.required}
                  className="create-form-input"
                />
              )}
            </div>
          ))}
          
          {error && (
            <div className="create-form-error">
              {error}
            </div>
          )}
          
          <div className="create-form-actions">
            <button
              type="button"
              onClick={onCancel}
              className="create-form-cancel"
              disabled={isSubmitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="create-form-submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Creating...' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateForm;
