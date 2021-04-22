import React, { useState } from 'react';
import { formatDate } from '../utils';
import TodoForm from './TodoForm';
import styles from './CreatorList.module.css';

export default function CreatorList({ todoList, onDelete }) {
  const [todoToUpdate, setTodoToUpdate] = useState(null);

  return (
    <>
      {todoToUpdate && (
        <div
          className={styles.popup}
          onClick={() => {
            setTodoToUpdate();
          }}
        >
          <div onClick={(event) => event.stopPropagation()}>
            <h2>Update:</h2>
            <TodoForm todo={todoToUpdate} onUpdate={() =>{
              setTodoToUpdate();
            }} />
          </div>
        </div>
      )}
      <ul className={styles.list}>
        <li>
          <strong>Task</strong> <strong>Done By</strong>
          <strong>Due date</strong>
          <strong>Delete</strong>
        </li>
        {todoList.map((todo) => {
          return (
            <li key={todo.id}>
              <div
                onClick={() => {
                  setTodoToUpdate(todo);
                }}
              >
                {todo.content}
              </div>
              <div className={styles.number}>{todo.usersDone?.length}</div>

              <time>{formatDate(todo.dueDateTime)}</time>
              <div>
                <button
                  onClick={(event) => {
                    onDelete(todo);
                  }}
                >
                  🗑
                </button>
              </div>
            </li>
          );
        })}
      </ul>
    </>
  );
}
