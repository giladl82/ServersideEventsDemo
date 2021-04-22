import React, { useState, useContext } from 'react';
import { AuthContext } from '../providers/Auth';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

import styles from './TodoForm.module.css';

export default function TodoForm({ todo, onUpdate }) {
  const defaultDate = new Date();
  defaultDate.setDate(defaultDate.getDate() + 14);
  const [content, setContent] = useState(todo ? todo.content : '');
  const [dateTime, setDateTime] = useState(todo ? new Date(todo.dueDateTime) : defaultDate);

  const auth = useContext(AuthContext);

  return (
    <form
      className={styles.form}
      onSubmit={async (event) => {
        event.preventDefault();
        await fetch(`api/Sse/${auth.currentRoom}`, {
          method: todo ? 'POST' : 'PUT',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            id: todo ? todo.id : null,
            content,
            dueDateTime: dateTime,
            usersDone: todo ? todo.usersDone : [],
          }),
        });
        setContent('');
        setDateTime(defaultDate);

        if (typeof onUpdate === 'function') {
          onUpdate();
        }
      }}
    >
      <input
        type="text"
        className={styles.content}
        placeholder="Enter a new todo task"
        value={content}
        onChange={(event) => {
          setContent(event.target.value);
        }}
      />
      <DatePicker
        className={styles.datePicker}
        popperClassName={styles.popper}
        calendarClassName={styles.calendar}
        showTimeSelect
        dateFormat="dd/MM/yyyy"
        selected={dateTime}
        onChange={(date) => {
          setDateTime(date);
        }}
      />
      <button>Save task</button>
    </form>
  );
}
