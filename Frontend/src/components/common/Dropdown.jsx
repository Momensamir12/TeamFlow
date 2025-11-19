import React, { useEffect, useRef } from 'react';

function Dropdown({ isOpen, onClose, children, trigger, align = 'right' }) {
  const dropdownRef = useRef(null);

  useEffect(() => {
    if (!isOpen) return;

    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        onClose();
      }
    };

    const handleEscape = (event) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleEscape);

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('keydown', handleEscape);
    };
  }, [isOpen, onClose]);

  const alignmentClasses = align === 'right' ? 'right-0' : 'left-0';

  return (
    <div className="relative inline-block" ref={dropdownRef}>
      {trigger}
      {isOpen && (
        <div className={`absolute ${alignmentClasses} mt-2 w-40 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-[100] overflow-hidden`}>
          {children}
        </div>
      )}
    </div>
  );
}

export function DropdownItem({ onClick, children, className = '' }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-indigo-50 hover:text-indigo-700 transition-colors ${className}`}
    >
      {children}
    </button>
  );
}

export default Dropdown;
