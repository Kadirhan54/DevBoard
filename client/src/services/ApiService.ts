export interface Project {
    id: string;
    name: string;
    description: string;
    tenantId: string;
  }

export interface TaskItem {
  id: string;
  name: string;
  description: string;
  status: number;
  dueDate: string;
  boardId: string;
}

export interface Board {
  id: string;
  name: string;
  description: string;
  tasks?: TaskItem[];
}

export interface ProjectWithBoards {
  id: string;
  name: string;
  description: string;
  boards: Board[];
}

export const fetchProjects = async (): Promise<Project[]> => {
  const response = await fetch('https://localhost:7018/api/projects');
  if (!response.ok) {
    throw new Error(`Failed to fetch projects. Status: ${response.status}`);
  }
  const data: Project[] = await response.json();
  return data;
};

export const fetchBoards = async (projectId: string): Promise<Board[]> => {
  const response = await fetch(`https://localhost:7018/api/projects/${projectId}`);
  if (!response.ok) {
    throw new Error(`Failed to fetch boards. Status: ${response.status}`);
  }
  const data: Board[] = await response.json();
  return data;
};

export const fetchProjectWithBoards = async (projectId: string): Promise<ProjectWithBoards> => {
  const response = await fetch(`https://localhost:7018/api/projects/${projectId}/boards`);
  if (!response.ok) {
    throw new Error(`Failed to fetch project with boards. Status: ${response.status}`);
  }
  const data: ProjectWithBoards = await response.json();
  return data;
};

export const createProject = async (projectData: { name: string; description: string; tenantId: string }): Promise<Project> => {
  const response = await fetch('https://localhost:7018/api/projects', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(projectData),
  });
  if (!response.ok) {
    throw new Error(`Failed to create project. Status: ${response.status}`);
  }
  const data: Project = await response.json();
  return data;
};

export const createBoard = async (projectId: string, boardData: { name: string; description: string }): Promise<Board> => {
  const response = await fetch(`https://localhost:7018/api/boards`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      ...boardData,
      projectId: projectId
    }),
  });
  if (!response.ok) {
    throw new Error(`Failed to create board. Status: ${response.status}`);
  }
  const data: Board = await response.json();
  return data;
};

export const createTask = async (boardId: string, taskData: { name: string; description: string; status: number; dueDate: string }): Promise<TaskItem> => {
  // Convert dueDate to ISO string format
  const formattedDueDate = new Date(taskData.dueDate).toISOString();
  
  const response = await fetch(`https://localhost:7018/api/taskItems`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      ...taskData,
      dueDate: formattedDueDate,
      boardId: boardId
    }),
  });
  if (!response.ok) {
    throw new Error(`Failed to create task. Status: ${response.status}`);
  }
  const data: TaskItem = await response.json();
  return data;
};
  