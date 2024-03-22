function navigateToProject(projectUrl, clickedProject) {
  clickedProject.style.transform = 'translateX(100vw)';
  
  setTimeout(function() {
    window.location.href = 'projects/' + projectUrl;
  }, 500);
}