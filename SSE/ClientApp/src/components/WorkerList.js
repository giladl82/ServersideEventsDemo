import React from 'react';
import styles from './WorkerList.module.css';
import { formatDate } from '../utils';

export default function WorkerList({ todoList, onToggle }) {
  return (
    <ul className={styles.list}>
      {todoList.map((todo) => {
        return (
          <li key={todo.id} className={todo.isDone ? styles.done : ''}>
            <label htmlFor={todo.id}>
              <input type="checkbox" checked={todo.isDone} id={todo.id} onChange={() => {
                onToggle(todo.id);
              }} />
              {todo.content}
            </label>
            <time>{formatDate(todo.dueDateTime)}</time>
          </li>
        );
      })}
    </ul>
  );
}
