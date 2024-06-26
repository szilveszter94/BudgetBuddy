/* eslint-disable react/prop-types */
const SelectComponent = ({value, text, array, id, onchange, className}) => {
  return (
    <select
      onChange={onchange}
      className={`form-control mb-3 ${className && className}`}
      value={value}
      required
      id={id}
      name={id}
    >
      <option disabled value="">
        {text}
      </option>
      {array.map((item, index) => (
        <option key={index} value={item}>
          {item}
        </option>
      ))}
    </select>
  );
};

export default SelectComponent;
