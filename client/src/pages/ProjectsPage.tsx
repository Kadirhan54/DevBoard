import React, { useEffect, useState } from 'react';
import { fetchProjects, createProject, type Project } from '../services/ApiService';
import { Link } from 'react-router-dom';
import CreateForm from '../components/CreateForm';
import './ProjectsPage.css';

const ProjectsPage: React.FC = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState<boolean>(false);

  useEffect(() => {
    const loadProjects = async () => {
      try {
        const data = await fetchProjects();
        setProjects(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    };

    loadProjects();
  }, []);

  const handleCreateProject = async (data: Record<string, any>) => {
    const newProject = await createProject({
      name: data.name,
      description: data.description,
      tenantId: data.tenantId || 'default-tenant' // You might want to get this from context/auth
    });
    setProjects(prev => [...prev, newProject]);
    setShowCreateForm(false);
  };


  if (loading) {
    return (
      <div className="projects-container">
        <div className="projects-loading">
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <div className="projects-loading-spinner" />
            Loading projects...
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="projects-container">
        <div className="projects-error">
          ⚠️ Error: {error}
        </div>
      </div>
    );
  }

  return (
    <div className="projects-container">
      <div className="projects-wrapper">
        <div className="projects-header">
          <h1 className="projects-title">Projects</h1>
          <p className="projects-subtitle">Manage and explore your development projects</p>
          <button 
            className="projects-create-button"
            onClick={() => setShowCreateForm(true)}
          >
            + Create Project
          </button>
        </div>
        
        {projects.length === 0 ? (
          <p className="projects-empty">No projects found.</p>
        ) : (
          <div className="projects-grid">
            {projects.map((project) => (
              <div key={project.id} className="project-card">
                <Link to={`/projects/${project.id}`} className="project-link">
                  <h3 className="project-name">{project.name}</h3>
                  <p className="project-description">{project.description}</p>
                  <p className="project-tenant">Tenant: {project.tenantId}</p>
                </Link>
              </div>
            ))}
          </div>
        )}
      </div>

      <CreateForm
        title="Create New Project"
        fields={[
          { name: 'name', label: 'Project Name', type: 'text', required: true },
          { name: 'description', label: 'Description', type: 'textarea', required: true },
          { name: 'tenantId', label: 'Tenant ID', type: 'text', required: true }
        ]}
        onSubmit={handleCreateProject}
        onCancel={() => setShowCreateForm(false)}
        isOpen={showCreateForm}
      />
    </div>
  );
};

export default ProjectsPage;
