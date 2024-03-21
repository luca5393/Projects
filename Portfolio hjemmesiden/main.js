function showProjectDetails(projectName) {
    document.getElementById('header').style.display = 'none';
    document.getElementById('projects-container').style.display = 'none';
    document.getElementById('details-container').style.display = 'block';
    document.getElementById('project-title').innerText = projectName;
    
    switch (projectName) {
      case 'KaosTek':
        document.getElementById('project-details').innerText = 'Details of Project 1';
        break;
      case 'Kajakklubben':
        document.getElementById('project-details').innerText = 'Details of Project 2';
        break;
      default:
        document.getElementById('project-details').innerText = 'Details not available';
    }
  }
  
  function goBack() {
    document.getElementById('header').style.display = 'block';
    document.getElementById('projects-container').style.display = 'block';
    document.getElementById('details-container').style.display = 'none';
  }
  