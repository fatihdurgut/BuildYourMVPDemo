// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Shopping cart functionality
let cart = [];
const apiGatewayUrl = 'https://localhost:7108';

document.addEventListener('DOMContentLoaded', () => {
    // Load cart from localStorage if it exists
    const savedCart = localStorage.getItem('cart');
    if (savedCart) {
        cart = JSON.parse(savedCart);
        updateCartBadge();
    }

    // Add click event listeners to all buy buttons
    const buyButtons = document.querySelectorAll('.buy-button');
    buyButtons.forEach(button => {
        button.addEventListener('click', handleAddToCart);
    });

    // Initialize axios default settings
    axios.defaults.baseURL = apiGatewayUrl;
    axios.defaults.headers.common['Accept'] = 'application/json';
    
    // Add axios response interceptor for error handling
    axios.interceptors.response.use(
        response => response,
        error => {
            console.error('API Error:', error);
            const notification = showNotification(
                'Error communicating with the server. Please try again later.',
                'error'
            );
            return Promise.reject(error);
        }
    );

    // Handle image loading animations
    const images = document.querySelectorAll('.card-img-top');
    images.forEach(img => {
        // Set initial opacity to 1
        img.style.opacity = '1';
        
        // Handle loading state
        if (!img.complete) {
            img.style.opacity = '0';
            img.addEventListener('load', () => {
                img.style.transition = 'opacity 0.3s ease';
                img.style.opacity = '1';
            });
        }

        // Handle image loading errors
        img.addEventListener('error', () => {
            img.src = '/images/placeholder.png';
            img.style.opacity = '1';
        });
    });

    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});

async function handleAddToCart(event) {
    const button = event.target;
    
    try {
        // Disable button during API call
        button.disabled = true;
        
        // Get current stock from API
        const productId = button.dataset.productId;
        const response = await axios.get(`/api/Product/${productId}`);
        const product = response.data;

        if (product.stockQuantity <= 0) {
            showNotification('Sorry, this item is out of stock.', 'warning');
            return;
        }

        const cartItem = {
            id: product.productId,
            name: product.name,
            price: product.price,
            image: product.imageUrl,
            quantity: 1
        };

        // Check if product already exists in cart
        const existingProductIndex = cart.findIndex(item => item.id === cartItem.id);
        
        if (existingProductIndex > -1) {
            // Check if adding another would exceed stock
            if (cart[existingProductIndex].quantity >= product.stockQuantity) {
                showNotification('Sorry, no more stock available for this item.', 'warning');
                return;
            }
            // Increment quantity if product already exists
            cart[existingProductIndex].quantity += 1;
        } else {
            // Add new product to cart
            cart.push(cartItem);
        }

        localStorage.setItem('cart', JSON.stringify(cart));
        updateCartBadge();

        // Animate the button and show feedback
        button.classList.add('btn-success');
        showNotification('Added to cart!', 'success');
        
        setTimeout(() => {
            button.classList.remove('btn-success');
        }, 2000);
    } catch (error) {
        console.error('Error adding to cart:', error);
        showNotification('Error adding item to cart. Please try again.', 'error');
    } finally {
        button.disabled = false;
    }
}

function updateCartBadge() {
    const badge = document.querySelector('.badge');
    if (badge) {
        const totalItems = cart.reduce((total, item) => total + item.quantity, 0);
        badge.textContent = totalItems;
        
        // Animate badge when updated
        badge.style.transform = 'scale(1.2)';
        setTimeout(() => {
            badge.style.transform = 'scale(1)';
        }, 200);
    }
}

function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    // Add notification styles
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 1rem;
        border-radius: 4px;
        color: white;
        z-index: 1000;
        animation: slideIn 0.3s ease forwards, fadeOut 0.3s ease 1.7s forwards;
    `;

    // Set background color based on type
    switch (type) {
        case 'success':
            notification.style.backgroundColor = '#4CAF50';
            break;
        case 'error':
            notification.style.backgroundColor = '#dc3545';
            break;
        case 'warning':
            notification.style.backgroundColor = '#ffc107';
            notification.style.color = '#000';
            break;
    }

    document.body.appendChild(notification);
    setTimeout(() => notification.remove(), 2000);
    return notification;
}

// Add hover effects to product cards
document.querySelectorAll('.card').forEach(card => {
    card.addEventListener('mouseenter', e => {
        e.currentTarget.style.transform = 'translateY(-10px)';
    });

    card.addEventListener('mouseleave', e => {
        e.currentTarget.style.transform = 'translateY(0)';
    });
});
