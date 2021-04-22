export function formatDate(date) {
  const formatDate = new Date(date);
  return (
    formatDate.getDate() +
    '/' +
    (formatDate.getMonth() + 1) +
    '/' +
    formatDate.getFullYear() +
    ' ' +
    formatDate.getHours() +
    ':' +
    formatDate.getMinutes()
  );
}