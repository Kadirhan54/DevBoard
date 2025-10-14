import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { type ProjectWithBoards, fetchProjectWithBoards, createBoard, createTask } from '../services/ApiService';
import CreateForm from '../components/CreateForm';
import './ProjectDetailsPage.css';

const ProjectDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [projectWithBoards, setProjectWithBoards] = useState<ProjectWithBoards | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateBoard, setShowCreateBoard] = useState(false);
  const [showCreateTask, setShowCreateTask] = useState(false);
  const [selectedBoardId, setSelectedBoardId] = useState<string | null>(null);

  const getStatusText = (status: number): string => {
    switch (status) {
      case 0: return 'Todo';
      case 1: return 'In Progress';
      case 2: return 'Done';
      default: return 'Unknown';
    }
  };

  const handleCreateBoard = async (data: Record<string, any>) => {
    if (!id) return;
    const newBoard = await createBoard(id, {
      name: data.name,
      description: data.description
    });
    // Ensure the board has an empty tasks array
    const boardWithTasks = { ...newBoard, tasks: newBoard.tasks || [] };
    setProjectWithBoards(prev => prev ? {
      ...prev,
      boards: [...prev.boards, boardWithTasks]
    } : null);
    setShowCreateBoard(false);
  };

  const handleCreateTask = async (data: Record<string, any>) => {
    if (!selectedBoardId) return;
    const newTask = await createTask(selectedBoardId, {
      name: data.name,
      description: data.description,
      status: parseInt(data.status),
      dueDate: data.dueDate
    });
    setProjectWithBoards(prev => prev ? {
      ...prev,
      boards: prev.boards.map(board => 
        board.id === selectedBoardId 
          ? { ...board, tasks: [...(board.tasks || []), newTask] }
          : board
      )
    } : null);
    setShowCreateTask(false);
    setSelectedBoardId(null);
  };

  const openCreateTask = (boardId: string) => {
    setSelectedBoardId(boardId);
    setShowCreateTask(true);
  };

  useEffect(() => {
    const fetchProjectWithBoardsData = async () => {
      try {
        const data = await fetchProjectWithBoards(id!);
        setProjectWithBoards(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchProjectWithBoardsData();
    }
  }, [id]);

  if (loading) {
    return (
      <div className="project-details-container">
        <div className="project-details-loading">
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <div className="project-details-loading-spinner" />
            Loading project...
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="project-details-container">
        <div className="project-details-error">
          ⚠️ Error: {error}
        </div>
      </div>
    );
  }

  if (!projectWithBoards) {
    return (
      <div className="project-details-container">
        <div className="project-details-error">
          Project not found.
        </div>
      </div>
    );
  }

  return (
    <div className="project-details-container">
      <Link to="/projects" className="project-details-back-button">
        ← Back to Projects
      </Link>
      
      <div className="project-details-card">
        <h1 className="project-details-title">{projectWithBoards.name}</h1>
        <p className="project-details-description">{projectWithBoards.description}</p>
        
        <div className="project-details-info-grid">
          <div className="project-details-info-card">
            <p className="project-details-info-label">Project ID</p>
            <p className="project-details-info-value">{projectWithBoards.id}</p>
          </div>
        </div>

        <div className="project-details-boards-section">
          <div className="project-details-boards-header">
            <h2 className="project-details-boards-title">Boards</h2>
            <button 
              className="project-details-create-board-button"
              onClick={() => setShowCreateBoard(true)}
            >
              + Create Board
            </button>
          </div>
          {projectWithBoards.boards.length === 0 ? (
            <div className="project-details-no-boards">
              No boards found for this project.
            </div>
          ) : (
            <div className="project-details-boards-grid">
              {projectWithBoards.boards.map((board) => (
                <div key={board.id} className="project-details-board-card">
                  <h3 className="project-details-board-name">{board.name}</h3>
                  <p className="project-details-board-description">{board.description}</p>
                  
                  <div className="project-details-board-task-items">
                    <div className="project-details-task-items-header">
                      <h4 className="project-details-task-items-title">
                        📋 Tasks ({board.tasks?.length || 0})
                      </h4>
                      <button 
                        className="project-details-create-task-button"
                        onClick={() => openCreateTask(board.id)}
                      >
                        + Add Task
                      </button>
                    </div>
                    {!board.tasks || board.tasks.length === 0 ? (
                      <div className="project-details-no-task-items">
                        No tasks in this board.
                      </div>
                    ) : (
                      <ul className="project-details-task-items-list">
                        {board.tasks.map((taskItem) => (
                          <li key={taskItem.id} className="project-details-task-item">
                            <h5 className="project-details-task-item-title">{taskItem.name}</h5>
                            <p className="project-details-task-item-description">{taskItem.description}</p>
                            <div className="project-details-task-item-meta">
                              <span className={`project-details-task-item-status status-${taskItem.status}`}>
                                {getStatusText(taskItem.status)}
                              </span>
                              <span className="project-details-task-item-due-date">
                                Due: {new Date(taskItem.dueDate).toLocaleDateString()}
                              </span>
                            </div>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <CreateForm
        title="Create New Board"
        fields={[
          { name: 'name', label: 'Board Name', type: 'text', required: true },
          { name: 'description', label: 'Description', type: 'textarea', required: true }
        ]}
        onSubmit={handleCreateBoard}
        onCancel={() => setShowCreateBoard(false)}
        isOpen={showCreateBoard}
      />

      <CreateForm
        title="Create New Task"
        fields={[
          { name: 'name', label: 'Task Name', type: 'text', required: true },
          { name: 'description', label: 'Description', type: 'textarea', required: true },
          { 
            name: 'status', 
            label: 'Status', 
            type: 'select', 
            required: true,
            options: [
              { value: 0, label: 'Todo' },
              { value: 1, label: 'In Progress' },
              { value: 2, label: 'Done' }
            ]
          },
          { name: 'dueDate', label: 'Due Date', type: 'date', required: true }
        ]}
        onSubmit={handleCreateTask}
        onCancel={() => {
          setShowCreateTask(false);
          setSelectedBoardId(null);
        }}
        isOpen={showCreateTask}
      />
    </div>
  );
};

export default ProjectDetailsPage;
